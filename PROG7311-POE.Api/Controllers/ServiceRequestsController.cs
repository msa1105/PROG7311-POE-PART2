using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG7311_POE.Data;
using PROG7311_POE.Models;
using PROG7311_POE.Services;

using Microsoft.AspNetCore.Authorization;

namespace PROG7311_POE.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrencyService _currencyService;
    private readonly IContractWorkflowService _workflowService;

    public ServiceRequestsController(ApplicationDbContext context,
        ICurrencyService currencyService,
        IContractWorkflowService workflowService)
    {
        _context = context;
        _currencyService = currencyService;
        _workflowService = workflowService;
    }

    // GET: api/ServiceRequestsApi
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetServiceRequests()
    {
        return await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c!.Client)
            .OrderByDescending(sr => sr.Id)
            .ToListAsync();
    }

    // GET: api/ServiceRequestsApi/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceRequest>> GetServiceRequest(int id)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c!.Client)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
        {
            return NotFound();
        }

        return serviceRequest;
    }

    // POST: api/ServiceRequestsApi
    [HttpPost]
    public async Task<ActionResult<ServiceRequest>> PostServiceRequest(ServiceRequest serviceRequest)
    {
        serviceRequest.Description = serviceRequest.Description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(serviceRequest.Description))
        {
            return BadRequest("Description is required.");
        }

        var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
        if (contract == null)
        {
            return BadRequest("The selected contract does not exist.");
        }

        // Validate contract workflow status
        try
        {
            _workflowService.ValidateServiceRequestCreation(contract);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        // Convert currency
        try
        {
            serviceRequest.LocalCost = await _currencyService.ConvertUsdToZarAsync(serviceRequest.Cost);
        }
        catch (Exception ex)
        {
            return StatusCode(503, $"Currency conversion failed: {ex.Message}");
        }

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetServiceRequest), new { id = serviceRequest.Id }, serviceRequest);
    }

    // PUT: api/ServiceRequestsApi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutServiceRequest(int id, ServiceRequest serviceRequest)
    {
        if (id != serviceRequest.Id)
        {
            return BadRequest("ID mismatch.");
        }

        serviceRequest.Description = serviceRequest.Description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(serviceRequest.Description))
        {
            return BadRequest("Description is required.");
        }

        var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
        if (contract == null)
        {
            return BadRequest("The selected contract does not exist.");
        }

        // Convert currency
        try
        {
            serviceRequest.LocalCost = await _currencyService.ConvertUsdToZarAsync(serviceRequest.Cost);
        }
        catch (Exception ex)
        {
            return StatusCode(503, $"Currency conversion failed: {ex.Message}");
        }

        _context.Entry(serviceRequest).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.ServiceRequests.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/ServiceRequestsApi/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServiceRequest(int id)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(id);
        if (serviceRequest == null)
        {
            return NotFound();
        }

        _context.ServiceRequests.Remove(serviceRequest);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
