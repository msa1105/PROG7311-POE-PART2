using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROG7311_POE.Data;
using PROG7311_POE.Models;
using PROG7311_POE.Models.ViewModels;
using PROG7311_POE.Services;

namespace PROG7311_POE.Controllers;

public class ContractsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileValidationService _fileValidation;
    private readonly IWebHostEnvironment _env;

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public ContractsController(ApplicationDbContext context,
        IFileValidationService fileValidation,
        IWebHostEnvironment env)
    {
        _context = context;
        _fileValidation = fileValidation;
        _env = env;
    }

    // Code attribution
    // Microsoft. 2023. Filtering with LINQ in EF Core.
    // Available at: https://learn.microsoft.com/en-us/ef/core/querying/
    // [Accessed: 15 January 2025]
    public async Task<IActionResult> Index(ContractFilterViewModel filter)
    {
        var query = _context.Contracts
            .Include(c => c.Client)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.ClientName))
            query = query.Where(c => c.Client!.Name.Contains(filter.ClientName));

        if (filter.Status.HasValue)
            query = query.Where(c => c.Status == filter.Status.Value);

        if (!string.IsNullOrWhiteSpace(filter.StartDateFrom) &&
            DateTime.TryParseExact(filter.StartDateFrom, "yy/MM/dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var sdFrom))
            query = query.Where(c => c.StartDate >= sdFrom);

        if (!string.IsNullOrWhiteSpace(filter.StartDateTo) &&
            DateTime.TryParseExact(filter.StartDateTo, "yy/MM/dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var sdTo))
            query = query.Where(c => c.StartDate <= sdTo);

        if (!string.IsNullOrWhiteSpace(filter.EndDateFrom) &&
            DateTime.TryParseExact(filter.EndDateFrom, "yy/MM/dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var edFrom))
            query = query.Where(c => c.EndDate >= edFrom);

        if (!string.IsNullOrWhiteSpace(filter.EndDateTo) &&
            DateTime.TryParseExact(filter.EndDateTo, "yy/MM/dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var edTo))
            query = query.Where(c => c.EndDate <= edTo);

        filter.Results = await query.OrderByDescending(c => c.StartDate).ToListAsync();
        return View(filter);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (contract == null) return NotFound();
        return View(contract);
    }

    public IActionResult Create()
    {
        PopulateClientDropdown();
        return View(new ContractFormViewModel());
    }

    // Code attribution
    // Microsoft. 2023. File Uploads in ASP.NET Core.
    // Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads
    // [Accessed: 15 January 2025]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractFormViewModel vm)
    {
        if (vm.AgreementFile != null)
        {
            if (vm.AgreementFile.Length > MaxFileSizeBytes)
                ModelState.AddModelError(nameof(vm.AgreementFile), "File must be 10 MB or smaller.");
            else
            {
                try { _fileValidation.IsValidPdf(vm.AgreementFile); }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(vm.AgreementFile), ex.Message);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            PopulateClientDropdown(vm.ClientId);
            return View(vm);
        }

        // Verify the selected client actually exists
        if (!await _context.Clients.AnyAsync(c => c.Id == vm.ClientId))
        {
            ModelState.AddModelError(nameof(vm.ClientId), "The selected client does not exist.");
            PopulateClientDropdown(vm.ClientId);
            return View(vm);
        }

        var contract = new Contract
        {
            ClientId = vm.ClientId,
            StartDate = vm.StartDate,
            EndDate = vm.EndDate,
            Status = vm.Status,
            ServiceLevel = vm.ServiceLevel?.Trim()
        };

        if (vm.AgreementFile != null)
        {
            try { contract.AgreementFilePath = await SaveFileAsync(vm.AgreementFile); }
            catch (IOException)
            {
                ModelState.AddModelError(nameof(vm.AgreementFile), "Failed to save the file. Please try again.");
                PopulateClientDropdown(vm.ClientId);
                return View(vm);
            }
        }

        try
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Contract created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save the contract. Please try again.");
            PopulateClientDropdown(vm.ClientId);
            return View(vm);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return NotFound();

        PopulateClientDropdown(contract.ClientId);
        return View(new ContractFormViewModel
        {
            Id = contract.Id,
            ClientId = contract.ClientId,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status,
            ServiceLevel = contract.ServiceLevel,
            ExistingAgreementFilePath = contract.AgreementFilePath
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContractFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (vm.AgreementFile != null)
        {
            if (vm.AgreementFile.Length > MaxFileSizeBytes)
                ModelState.AddModelError(nameof(vm.AgreementFile), "File must be 10 MB or smaller.");
            else
            {
                try { _fileValidation.IsValidPdf(vm.AgreementFile); }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(vm.AgreementFile), ex.Message);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            PopulateClientDropdown(vm.ClientId);
            return View(vm);
        }

        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return NotFound();

        contract.ClientId = vm.ClientId;
        contract.StartDate = vm.StartDate;
        contract.EndDate = vm.EndDate;
        contract.Status = vm.Status;
        contract.ServiceLevel = vm.ServiceLevel?.Trim();

        if (vm.AgreementFile != null)
        {
            try { contract.AgreementFilePath = await SaveFileAsync(vm.AgreementFile); }
            catch (IOException)
            {
                ModelState.AddModelError(nameof(vm.AgreementFile), "Failed to save the file. Please try again.");
                PopulateClientDropdown(vm.ClientId);
                return View(vm);
            }
        }

        try
        {
            _context.Update(contract);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Contract #{id} updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Contracts.AnyAsync(c => c.Id == id)) return NotFound();
            ModelState.AddModelError(string.Empty, "This record was modified by another user. Please reload and try again.");
            PopulateClientDropdown(vm.ClientId);
            return View(vm);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save changes. Please try again.");
            PopulateClientDropdown(vm.ClientId);
            return View(vm);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (contract == null) return NotFound();
        return View(contract);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return NotFound();

        try
        {
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Contract #{id} was deleted.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Unable to delete this contract. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ViewAgreement(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null || string.IsNullOrEmpty(contract.AgreementFilePath))
            return NotFound();

        var physicalPath = Path.Combine(_env.WebRootPath, contract.AgreementFilePath.TrimStart('/'));
        if (!System.IO.File.Exists(physicalPath))
            return NotFound("The agreement file could not be found on the server.");

        return PhysicalFile(physicalPath, "application/pdf");
    }

    // Code attribution
    // Microsoft. 2023. Saving Files in ASP.NET Core / IFormFile.
    // Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads
    // [Accessed: 15 January 2025]
    private async Task<string> SaveFileAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
        Directory.CreateDirectory(uploadsFolder);

        // GUID naming prevents overwriting: contract_<guid>.pdf
        var guid = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueName = $"contract_{guid}{extension}";

        var filePath = Path.Combine(uploadsFolder, uniqueName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/contracts/{uniqueName}";
    }

    private void PopulateClientDropdown(int? selectedId = null)
    {
        ViewBag.Clients = new SelectList(
            _context.Clients.OrderBy(c => c.Name),
            "Id", "Name", selectedId);
    }
}
