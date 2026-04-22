using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG7311_POE.Data;
using PROG7311_POE.Models;

namespace PROG7311_POE.Controllers;

public class ClientsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Code attribution
    // Microsoft. 2023. Loading Related Data - EF Core.
    // Available at: https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager
    // [Accessed: 15 January 2025]
    // Include(c => c.Contracts) is required here so that c.Contracts.Count is populated
    // in the view — without it EF returns an empty collection (lazy loading is off by default).
    public async Task<IActionResult> Index()
    {
        var clients = await _context.Clients
            .Include(c => c.Contracts)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(clients);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var client = await _context.Clients
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null) return NotFound();
        return View(client);
    }

    public IActionResult Create() => View();

    // Code attribution
    // Microsoft. 2023. Model Validation in ASP.NET Core MVC.
    // Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
    // [Accessed: 15 January 2025]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,ContactDetails,Region")] Client client)
    {
        // Trim whitespace before validation
        client.Name = client.Name?.Trim() ?? string.Empty;
        client.Region = client.Region?.Trim() ?? string.Empty;
        client.ContactDetails = client.ContactDetails?.Trim() ?? string.Empty;

        if (!ModelState.IsValid) return View(client);

        // Check for a duplicate client name (case-insensitive)
        bool nameExists = await _context.Clients
            .AnyAsync(c => c.Name.ToLower() == client.Name.ToLower());
        if (nameExists)
        {
            ModelState.AddModelError(nameof(client.Name), "A client with this name already exists.");
            return View(client);
        }

        try
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Client \"{client.Name}\" was created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save the client. Please try again.");
            return View(client);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return NotFound();
        return View(client);
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

        // Check for duplicate name, excluding this record itself
        bool nameExists = await _context.Clients
            .AnyAsync(c => c.Name.ToLower() == client.Name.ToLower() && c.Id != id);
        if (nameExists)
        {
            ModelState.AddModelError(nameof(client.Name), "Another client with this name already exists.");
            return View(client);
        }

        try
        {
            _context.Update(client);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Client \"{client.Name}\" was updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Clients.AnyAsync(c => c.Id == id)) return NotFound();
            ModelState.AddModelError(string.Empty, "This record was modified by another user. Please reload and try again.");
            return View(client);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Unable to save changes. Please try again.");
            return View(client);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var client = await _context.Clients
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (client == null) return NotFound();
        return View(client);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = await _context.Clients
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null) return NotFound();

        // Block deletion if the client still has contracts linked to them
        if (client.Contracts.Any())
        {
            TempData["Error"] = $"Cannot delete \"{client.Name}\" because they have {client.Contracts.Count} contract(s) attached. Remove the contracts first.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Client \"{client.Name}\" was deleted.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Unable to delete this client. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }
}
