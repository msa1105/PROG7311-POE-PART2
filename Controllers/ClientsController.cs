using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using PROG7311_POE.Models;
using Microsoft.AspNetCore.Authorization;

namespace PROG7311_POE.Controllers;

[Authorize]
public class ClientsController : Controller
{
    private readonly HttpClient _client;

    public ClientsController(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient("GlmsApi");
    }

    public async Task<IActionResult> Index()
    {
        var response = await _client.GetAsync("api/Clients");
        if (response.IsSuccessStatusCode)
        {
            var clients = await response.Content.ReadFromJsonAsync<List<Client>>();
            return View(clients ?? new List<Client>());
        }

        TempData["Error"] = "Unable to fetch clients from server.";
        return View(new List<Client>());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var response = await _client.GetAsync($"api/Clients/{id}");
        if (response.IsSuccessStatusCode)
        {
            var client = await response.Content.ReadFromJsonAsync<Client>();
            if (client == null) return NotFound();
            return View(client);
        }

        return NotFound();
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,ContactDetails,Region")] Client client)
    {
        client.Name = client.Name?.Trim() ?? string.Empty;
        client.Region = client.Region?.Trim() ?? string.Empty;
        client.ContactDetails = client.ContactDetails?.Trim() ?? string.Empty;

        if (!ModelState.IsValid) return View(client);

        var response = await _client.PostAsJsonAsync("api/Clients", client);
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"Client \"{client.Name}\" was created successfully.";
            return RedirectToAction(nameof(Index));
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to save the client. Please try again.");
        return View(client);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var response = await _client.GetAsync($"api/Clients/{id}");
        if (response.IsSuccessStatusCode)
        {
            var client = await response.Content.ReadFromJsonAsync<Client>();
            if (client == null) return NotFound();
            return View(client);
        }
        return NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactDetails,Region")] Client client)
    {
        if (id != client.Id) return NotFound();

        client.Name = client.Name?.Trim() ?? string.Empty;
        client.Region = client.Region?.Trim() ?? string.Empty;
        client.ContactDetails = client.ContactDetails?.Trim() ?? string.Empty;

        if (!ModelState.IsValid) return View(client);

        var response = await _client.PutAsJsonAsync($"api/Clients/{id}", client);
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"Client \"{client.Name}\" was updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to save changes. Please try again.");
        return View(client);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var response = await _client.GetAsync($"api/Clients/{id}");
        if (response.IsSuccessStatusCode)
        {
            var client = await response.Content.ReadFromJsonAsync<Client>();
            if (client == null) return NotFound();
            return View(client);
        }
        return NotFound();
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var response = await _client.DeleteAsync($"api/Clients/{id}");
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Client was deleted.";
            return RedirectToAction(nameof(Index));
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        TempData["Error"] = !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to delete client.";
        return RedirectToAction(nameof(Index));
    }
}
