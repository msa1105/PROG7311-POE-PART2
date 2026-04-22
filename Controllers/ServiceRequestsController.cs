using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROG7311_POE.Data;
using PROG7311_POE.Models;
using PROG7311_POE.Models.ViewModels;
using PROG7311_POE.Services;

namespace PROG7311_POE.Controllers;

public class ServiceRequestsController : Controller
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

    public async Task<IActionResult> Index()
    {
        var requests = await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c!.Client)
            .OrderByDescending(sr => sr.Id)
            .ToListAsync();
        return View(requests);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var sr = await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c!.Client)
            .FirstOrDefaultAsync(sr => sr.Id == id);
        if (sr == null) return NotFound();
        return View(sr);
    }

    public IActionResult Create(int? contractId)
    {
        PopulateContractDropdown(contractId);
        return View(new ServiceRequestFormViewModel { ContractId = contractId ?? 0 });
    }

    // Code attribution
    // Microsoft. 2023. Model Validation in ASP.NET Core MVC.
    // Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
    // [Accessed: 15 January 2025]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestFormViewModel vm)
    {
        // Trim description before validation so whitespace-only strings fail MinimumLength
        vm.Description = vm.Description?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
        {
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }

        var contract = await _context.Contracts.FindAsync(vm.ContractId);
        if (contract == null)
        {
            ModelState.AddModelError(nameof(vm.ContractId), "The selected contract does not exist.");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }

        // Business rule: contract status must allow new service requests
        try
        {
            _workflowService.ValidateServiceRequestCreation(contract);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }

        // Code attribution
        // OpenExchangeRates. 2024. API Documentation.
        // Available at: https://docs.openexchangerates.org/reference/api-introduction
        // [Accessed: 15 January 2025]
        decimal localCost;
        try
        {
            localCost = await _currencyService.ConvertUsdToZarAsync(vm.Cost);
        }
        catch (InvalidOperationException ex)
        {
            // API key invalid or quota exceeded
            ModelState.AddModelError(string.Empty, $"Currency conversion failed: {ex.Message}");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Currency conversion is temporarily unavailable. Please try again later.");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }

        var serviceRequest = new ServiceRequest
        {
            ContractId = vm.ContractId,
            Description = vm.Description,
            Cost = vm.Cost,
            LocalCost = localCost,
            Status = string.IsNullOrWhiteSpace(vm.Status) ? "Pending" : vm.Status.Trim()
        };

        try
        {
            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Service request created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save the service request. Please try again.");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var sr = await _context.ServiceRequests.FindAsync(id);
        if (sr == null) return NotFound();

        PopulateContractDropdown(sr.ContractId);
        return View(new ServiceRequestFormViewModel
        {
            Id = sr.Id,
            ContractId = sr.ContractId,
            Description = sr.Description,
            Cost = sr.Cost,
            Status = sr.Status
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceRequestFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        vm.Description = vm.Description?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
        {
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }

        var sr = await _context.ServiceRequests.FindAsync(id);
        if (sr == null) return NotFound();

        decimal localCost;
        try
        {
            localCost = await _currencyService.ConvertUsdToZarAsync(vm.Cost);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, $"Currency conversion failed: {ex.Message}");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Currency conversion is temporarily unavailable. Please try again later.");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }

        sr.ContractId = vm.ContractId;
        sr.Description = vm.Description;
        sr.Cost = vm.Cost;
        sr.LocalCost = localCost;
        sr.Status = string.IsNullOrWhiteSpace(vm.Status) ? "Pending" : vm.Status.Trim();

        try
        {
            _context.Update(sr);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Service request #{id} updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.ServiceRequests.AnyAsync(s => s.Id == id)) return NotFound();
            ModelState.AddModelError(string.Empty, "This record was modified by another user. Please reload and try again.");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save changes. Please try again.");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var sr = await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c!.Client)
            .FirstOrDefaultAsync(sr => sr.Id == id);
        if (sr == null) return NotFound();
        return View(sr);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sr = await _context.ServiceRequests.FindAsync(id);
        if (sr == null) return NotFound();

        try
        {
            _context.ServiceRequests.Remove(sr);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Service request #{id} was deleted.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Unable to delete this service request. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Code attribution
    // Microsoft. 2023. SelectList and DropDownList in ASP.NET Core MVC.
    // Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms#the-select-tag-helper
    // [Accessed: 15 January 2025]
    //
    // Code attribution
    // Stack Overflow. 2011. Populate a SelectList in ASP.NET MVC from a LINQ query.
    // Available at: https://stackoverflow.com/questions/5555730/asp-net-mvc-select-list-from-linq
    // [Accessed: 15 January 2025]
    private void PopulateContractDropdown(int? selectedId = null)
    {
        var contracts = _context.Contracts
            .Include(c => c.Client)
            .OrderBy(c => c.Client!.Name)
            .Select(c => new
            {
                c.Id,
                Display = c.Client!.Name + " - Contract #" + c.Id + " (" + c.Status + ")"
            })
            .ToList();
        ViewBag.Contracts = new SelectList(contracts, "Id", "Display", selectedId);
    }
}
