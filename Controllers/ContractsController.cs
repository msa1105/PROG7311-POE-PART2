using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROG7311_POE.Models;
using PROG7311_POE.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PROG7311_POE.Controllers;

[Authorize]
public class ContractsController : Controller
{
    private readonly HttpClient _client;

    public ContractsController(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient("GlmsApi");
    }

    public async Task<IActionResult> Index(ContractFilterViewModel filter)
    {
        var url = $"api/Contracts?clientName={System.Uri.EscapeDataString(filter.ClientName ?? "")}";
        if (filter.Status.HasValue)
        {
            url += $"&status={(int)filter.Status.Value}";
        }

        var response = await _client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var results = await response.Content.ReadFromJsonAsync<List<Contract>>();
            var query = (results ?? new List<Contract>()).AsQueryable();

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

            filter.Results = query.OrderByDescending(c => c.StartDate).ToList();
        }
        else
        {
            TempData["Error"] = "Unable to retrieve contracts from the API.";
        }

        return View(filter);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        
        var response = await _client.GetAsync($"api/Contracts/{id}");
        if (response.IsSuccessStatusCode)
        {
            var contract = await response.Content.ReadFromJsonAsync<Contract>();
            if (contract == null) return NotFound();
            return View(contract);
        }
        return NotFound();
    }

    public async Task<IActionResult> Create()
    {
        await PopulateClientDropdownAsync();
        return View(new ContractFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateClientDropdownAsync(vm.ClientId);
            return View(vm);
        }

        var clientResponse = await _client.GetAsync($"api/Clients/{vm.ClientId}");
        if (!clientResponse.IsSuccessStatusCode)
        {
            ModelState.AddModelError(nameof(vm.ClientId), "The selected client does not exist.");
            await PopulateClientDropdownAsync(vm.ClientId);
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

        var response = await _client.PostAsJsonAsync("api/Contracts", contract);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to save the contract. Please try again.");
            await PopulateClientDropdownAsync(vm.ClientId);
            return View(vm);
        }

        var createdContract = await response.Content.ReadFromJsonAsync<Contract>();
        if (createdContract == null)
        {
            ModelState.AddModelError(string.Empty, "Error deserializing contract from server.");
            await PopulateClientDropdownAsync(vm.ClientId);
            return View(vm);
        }

        if (vm.AgreementFile != null && vm.AgreementFile.Length > 0)
        {
            using (var content = new MultipartFormDataContent())
            {
                using (var stream = vm.AgreementFile.OpenReadStream())
                {
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(vm.AgreementFile.ContentType);
                    content.Add(fileContent, "file", vm.AgreementFile.FileName);

                    var uploadResponse = await _client.PostAsync($"api/Contracts/{createdContract.Id}/upload", content);
                    if (!uploadResponse.IsSuccessStatusCode)
                    {
                        var uploadError = await uploadResponse.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Contract created, but file upload failed: {uploadError}";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
        }

        TempData["Success"] = "Contract created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        
        var response = await _client.GetAsync($"api/Contracts/{id}");
        if (response.IsSuccessStatusCode)
        {
            var contract = await response.Content.ReadFromJsonAsync<Contract>();
            if (contract == null) return NotFound();

            await PopulateClientDropdownAsync(contract.ClientId);
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
        return NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContractFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateClientDropdownAsync(vm.ClientId);
            return View(vm);
        }

        var clientResponse = await _client.GetAsync($"api/Clients/{vm.ClientId}");
        if (!clientResponse.IsSuccessStatusCode)
        {
            ModelState.AddModelError(nameof(vm.ClientId), "The selected client does not exist.");
            await PopulateClientDropdownAsync(vm.ClientId);
            return View(vm);
        }

        var contract = new Contract
        {
            Id = vm.Id,
            ClientId = vm.ClientId,
            StartDate = vm.StartDate,
            EndDate = vm.EndDate,
            Status = vm.Status,
            ServiceLevel = vm.ServiceLevel?.Trim(),
            AgreementFilePath = vm.ExistingAgreementFilePath
        };

        var response = await _client.PutAsJsonAsync($"api/Contracts/{id}", contract);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to update the contract. Please try again.");
            await PopulateClientDropdownAsync(vm.ClientId);
            return View(vm);
        }

        if (vm.AgreementFile != null && vm.AgreementFile.Length > 0)
        {
            using (var content = new MultipartFormDataContent())
            {
                using (var stream = vm.AgreementFile.OpenReadStream())
                {
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(vm.AgreementFile.ContentType);
                    content.Add(fileContent, "file", vm.AgreementFile.FileName);

                    var uploadResponse = await _client.PostAsync($"api/Contracts/{id}/upload", content);
                    if (!uploadResponse.IsSuccessStatusCode)
                    {
                        var uploadError = await uploadResponse.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Contract metadata updated, but file upload failed: {uploadError}";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
        }

        TempData["Success"] = $"Contract #{id} updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        
        var response = await _client.GetAsync($"api/Contracts/{id}");
        if (response.IsSuccessStatusCode)
        {
            var contract = await response.Content.ReadFromJsonAsync<Contract>();
            if (contract == null) return NotFound();
            return View(contract);
        }
        return NotFound();
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var response = await _client.DeleteAsync($"api/Contracts/{id}");
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"Contract #{id} was deleted.";
        }
        else
        {
            TempData["Error"] = "Unable to delete the contract. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ViewAgreement(int id)
    {
        var response = await _client.GetAsync($"api/Contracts/{id}/download");
        if (!response.IsSuccessStatusCode)
        {
            return NotFound("The agreement file could not be found or retrieved from the server.");
        }

        var fileBytes = await response.Content.ReadAsByteArrayAsync();
        return File(fileBytes, "application/pdf");
    }

    private async Task PopulateClientDropdownAsync(int? selectedId = null)
    {
        var response = await _client.GetAsync("api/Clients");
        var clients = new List<Client>();
        if (response.IsSuccessStatusCode)
        {
            clients = await response.Content.ReadFromJsonAsync<List<Client>>() ?? new List<Client>();
        }
        ViewBag.Clients = new SelectList(clients.OrderBy(c => c.Name), "Id", "Name", selectedId);
    }
}
