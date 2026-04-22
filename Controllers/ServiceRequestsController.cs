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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }

        var contract = await _context.Contracts.FindAsync(vm.ContractId);
        if (contract == null)
        {
            ModelState.AddModelError("ContractId", "Selected contract does not exist.");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }

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

        decimal localCost;
        try
        {
            localCost = await _currencyService.ConvertUsdToZarAsync(vm.Cost);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Currency conversion failed: {ex.Message}");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }

        var serviceRequest = new ServiceRequest
        {
            ContractId = vm.ContractId,
            Description = vm.Description,
            Cost = vm.Cost,
            LocalCost = localCost,
            Status = vm.Status ?? "Pending"
        };

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
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
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Currency conversion failed: {ex.Message}");
            PopulateContractDropdown(vm.ContractId);
            return View(vm);
        }

        sr.ContractId = vm.ContractId;
        sr.Description = vm.Description;
        sr.Cost = vm.Cost;
        sr.LocalCost = localCost;
        sr.Status = vm.Status;

        try
        {
            _context.Update(sr);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.ServiceRequests.Any(s => s.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var sr = await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .FirstOrDefaultAsync(sr => sr.Id == id);
        if (sr == null) return NotFound();
        return View(sr);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sr = await _context.ServiceRequests.FindAsync(id);
        if (sr != null) _context.ServiceRequests.Remove(sr);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void PopulateContractDropdown(int? selectedId = null)
    {
        var contracts = _context.Contracts
            .Include(c => c.Client)
            .Select(c => new
            {
                c.Id,
                Display = c.Client!.Name + " — Contract #" + c.Id
            })
            .ToList();
        ViewBag.Contracts = new SelectList(contracts, "Id", "Display", selectedId);
    }
}
