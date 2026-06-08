using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG7311_POE.Data;
using PROG7311_POE.Models;

using Microsoft.AspNetCore.Authorization;

namespace PROG7311_POE.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ClientsApi
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Client>>> GetClients()
    {
        return await _context.Clients
            .Include(c => c.Contracts)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // GET: api/ClientsApi/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetClient(int id)
    {
        var client = await _context.Clients
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound();
        }

        return client;
    }

    // POST: api/ClientsApi
    [HttpPost]
    public async Task<ActionResult<Client>> PostClient(Client client)
    {
        client.Name = client.Name?.Trim() ?? string.Empty;
        client.Region = client.Region?.Trim() ?? string.Empty;
        client.ContactDetails = client.ContactDetails?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(client.Name))
        {
            return BadRequest("Client Name is required.");
        }

        // Duplicate name detection
        bool nameExists = await _context.Clients
            .AnyAsync(c => c.Name.ToLower() == client.Name.ToLower());
        if (nameExists)
        {
            return BadRequest("A client with this name already exists.");
        }

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
    }

    // PUT: api/ClientsApi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutClient(int id, Client client)
    {
        if (id != client.Id)
        {
            return BadRequest("ID mismatch.");
        }

        client.Name = client.Name?.Trim() ?? string.Empty;
        client.Region = client.Region?.Trim() ?? string.Empty;
        client.ContactDetails = client.ContactDetails?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(client.Name))
        {
            return BadRequest("Client Name is required.");
        }

        // Duplicate name detection (excluding self)
        bool nameExists = await _context.Clients
            .AnyAsync(c => c.Name.ToLower() == client.Name.ToLower() && c.Id != id);
        if (nameExists)
        {
            return BadRequest("Another client with this name already exists.");
        }

        _context.Entry(client).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Clients.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/ClientsApi/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound();
        }

        // Validate deletion rules
        if (client.Contracts.Any())
        {
            return BadRequest($"Cannot delete \"{client.Name}\" because they have active contracts linked. Remove the contracts first.");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
