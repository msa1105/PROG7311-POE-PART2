using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROG7311_POE.Models;
using PROG7311_POE.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;

namespace PROG7311_POE.Controllers;

[Authorize]
public class ServiceRequestsController : Controller
{
    private readonly HttpClient _client;

    public ServiceRequestsController(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient("GlmsApi");
    }

    public async Task<IActionResult> Index()
    {
        var response = await _client.GetAsync("api/ServiceRequests");
        if (response.IsSuccessStatusCode)
        {
            var requests = await response.Content.ReadFromJsonAsync<List<ServiceRequest>>();
            return View(requests ?? new List<ServiceRequest>());
        }

        TempData["Error"] = "Unable to retrieve service requests from the API.";
        return View(new List<ServiceRequest>());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var response = await _client.GetAsync($"api/ServiceRequests/{id}");
        if (response.IsSuccessStatusCode)
        {
            var sr = await response.Content.ReadFromJsonAsync<ServiceRequest>();
            if (sr == null) return NotFound();
            return View(sr);
        }

        return NotFound();
    }

    public async Task<IActionResult> Create(int? contractId)
    {
        await PopulateContractDropdownAsync(contractId);
        return View(new ServiceRequestFormViewModel { ContractId = contractId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestFormViewModel vm)
    {
        vm.Description = vm.Description?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
        {
            await PopulateContractDropdownAsync(vm.ContractId);
            return View(vm);
        }

        var serviceRequest = new ServiceRequest
        {
            ContractId = vm.ContractId,
            Description = vm.Description,
            Cost = vm.Cost,
            Status = string.IsNullOrWhiteSpace(vm.Status) ? "Pending" : vm.Status.Trim()
        };

        var response = await _client.PostAsJsonAsync("api/ServiceRequests", serviceRequest);
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Service request created successfully.";
            return RedirectToAction(nameof(Index));
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to save the service request. Please try again.");
        await PopulateContractDropdownAsync(vm.ContractId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var response = await _client.GetAsync($"api/ServiceRequests/{id}");
        if (response.IsSuccessStatusCode)
        {
            var sr = await response.Content.ReadFromJsonAsync<ServiceRequest>();
            if (sr == null) return NotFound();

            await PopulateContractDropdownAsync(sr.ContractId);
            return View(new ServiceRequestFormViewModel
            {
                Id = sr.Id,
                ContractId = sr.ContractId,
                Description = sr.Description,
                Cost = sr.Cost,
                Status = sr.Status
            });
        }

        return NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceRequestFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        vm.Description = vm.Description?.Trim() ?? string.Empty;

        if (!ModelState.IsValid)
        {
            await PopulateContractDropdownAsync(vm.ContractId);
            return View(vm);
        }

        var serviceRequest = new ServiceRequest
        {
            Id = vm.Id,
            ContractId = vm.ContractId,
            Description = vm.Description,
            Cost = vm.Cost,
            Status = string.IsNullOrWhiteSpace(vm.Status) ? "Pending" : vm.Status.Trim()
        };

        var response = await _client.PutAsJsonAsync($"api/ServiceRequests/{id}", serviceRequest);
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"Service request #{id} updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to update the service request. Please try again.");
        await PopulateContractDropdownAsync(vm.ContractId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var response = await _client.GetAsync($"api/ServiceRequests/{id}");
        if (response.IsSuccessStatusCode)
        {
            var sr = await response.Content.ReadFromJsonAsync<ServiceRequest>();
            if (sr == null) return NotFound();
            return View(sr);
        }

        return NotFound();
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var response = await _client.DeleteAsync($"api/ServiceRequests/{id}");
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"Service request #{id} was deleted.";
        }
        else
        {
            TempData["Error"] = "Unable to delete the service request. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateContractDropdownAsync(int? selectedId = null)
    {
        var response = await _client.GetAsync("api/Contracts");
        var list = new List<object>();
        if (response.IsSuccessStatusCode)
        {
            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>() ?? new List<Contract>();
            var dropdownItems = contracts
                .OrderBy(c => c.Client?.Name ?? string.Empty)
                .Select(c => new
                {
                    Id = c.Id,
                    Display = (c.Client?.Name ?? "Unknown Client") + " - Contract #" + c.Id + " (" + c.Status + ")"
                })
                .ToList();
            ViewBag.Contracts = new SelectList(dropdownItems, "Id", "Display", selectedId);
        }
        else
        {
            ViewBag.Contracts = new SelectList(list, "Id", "Display");
        }
    }
}
