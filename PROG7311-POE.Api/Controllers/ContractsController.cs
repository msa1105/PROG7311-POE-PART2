using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG7311_POE.Data;
using PROG7311_POE.Models;

using Microsoft.AspNetCore.Authorization;

using PROG7311_POE.Services;

namespace PROG7311_POE.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileValidationService _fileValidation;
    private readonly IWebHostEnvironment _env;

    public ContractsController(ApplicationDbContext context,
        IFileValidationService fileValidation,
        IWebHostEnvironment env)
    {
        _context = context;
        _fileValidation = fileValidation;
        _env = env;
    }

    // GET: api/ContractsApi
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Contract>>> GetContracts([FromQuery] string? clientName, [FromQuery] ContractStatus? status)
    {
        var query = _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(clientName))
        {
            query = query.Where(c => c.Client!.Name.Contains(clientName));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        return await query.OrderByDescending(c => c.StartDate).ToListAsync();
    }

    // GET: api/ContractsApi/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Contract>> GetContract(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
        {
            return NotFound();
        }

        return contract;
    }

    // POST: api/ContractsApi
    [HttpPost]
    public async Task<ActionResult<Contract>> PostContract(Contract contract)
    {
        if (contract.StartDate > contract.EndDate)
        {
            return BadRequest("Start Date cannot be after End Date.");
        }

        // Verify client exists
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == contract.ClientId);
        if (!clientExists)
        {
            return BadRequest("The selected client does not exist.");
        }

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contract);
    }

    // PUT: api/ContractsApi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutContract(int id, Contract contract)
    {
        if (id != contract.Id)
        {
            return BadRequest("ID mismatch.");
        }

        if (contract.StartDate > contract.EndDate)
        {
            return BadRequest("Start Date cannot be after End Date.");
        }

        var clientExists = await _context.Clients.AnyAsync(c => c.Id == contract.ClientId);
        if (!clientExists)
        {
            return BadRequest("The selected client does not exist.");
        }

        _context.Entry(contract).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Contracts.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/ContractsApi/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null)
        {
            return NotFound();
        }

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/Contracts/5/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> PatchStatus(int id, [FromBody] ContractStatus status)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null)
        {
            return NotFound();
        }

        contract.Status = status;

        // Trigger the Observer Pattern!
        var subject = new PROG7311_POE.Patterns.Observer.ContractSubject();
        var logisticsObserver = new PROG7311_POE.Patterns.Observer.LogisticsManagerObserver("demo-manager");
        var complianceObserver = new PROG7311_POE.Patterns.Observer.ComplianceObserver();

        subject.Attach(logisticsObserver);
        subject.Attach(complianceObserver);

        subject.SetStatus(status);

        await _context.SaveChangesAsync();
        return Ok(contract);
    }

    // POST: api/Contracts/{id}/upload
    [HttpPost("{id}/upload")]
    public async Task<IActionResult> UploadAgreement(int id, IFormFile file)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null)
        {
            return NotFound("Contract not found.");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            _fileValidation.IsValidPdf(file);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads", "contracts");
        Directory.CreateDirectory(uploadsFolder);

        var guid = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueName = $"contract_{guid}{extension}";

        var filePath = Path.Combine(uploadsFolder, uniqueName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        contract.AgreementFilePath = $"/uploads/contracts/{uniqueName}";
        await _context.SaveChangesAsync();

        return Ok(new { filePath = contract.AgreementFilePath });
    }

    // GET: api/Contracts/{id}/download
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null || string.IsNullOrEmpty(contract.AgreementFilePath))
        {
            return NotFound("Contract or agreement file not found.");
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var physicalPath = Path.Combine(webRoot, contract.AgreementFilePath.TrimStart('/'));
        if (!System.IO.File.Exists(physicalPath))
        {
            return NotFound("The agreement file could not be found on the server.");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
        return File(fileBytes, "application/pdf", Path.GetFileName(physicalPath));
    }
}

