//Code attribution
//OpenAI. 2025. ChatGPT (Version GPT-4). [Large language model]
//Available at: https://chat.openai.com/
//[Accessed: 15 January 2025]

using PROG7311_POE.Models;

namespace PROG7311_POE.Data;

/// <summary>
/// Seeds the database with initial mock data for testing purposes.
/// Ensures a minimum of 10 records across Clients, Contracts, and ServiceRequests.
/// All date formats strictly adhere to yy/MM/dd format.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if data already exists
        if (context.Clients.Any())
            return;

        // Create 5 Clients
        var clients = new List<Client>
        {
            new Client
            {
                Name = "Acme Corporation",
                ContactDetails = "john.doe@acme.com | +27 11 123 4567",
                Region = "Gauteng"
            },
            new Client
            {
                Name = "Global Tech Solutions",
                ContactDetails = "sarah.smith@globaltech.co.za | +27 21 987 6543",
                Region = "Western Cape"
            },
            new Client
            {
                Name = "Premier Consulting Group",
                ContactDetails = "mike.jones@premier.co.za | +27 31 555 1234",
                Region = "KwaZulu-Natal"
            },
            new Client
            {
                Name = "Innovation Labs SA",
                ContactDetails = "info@innovationlabs.co.za | +27 12 444 8888",
                Region = "Gauteng"
            },
            new Client
            {
                Name = "Coastal Enterprises",
                ContactDetails = "contact@coastal.co.za | +27 41 222 3333",
                Region = "Eastern Cape"
            }
        };

        context.Clients.AddRange(clients);
        await context.SaveChangesAsync();

        // Create 5 Contracts with date format yy/MM/dd
        var contracts = new List<Contract>
        {
            new Contract
            {
                ClientId = clients[0].Id,
                StartDate = new DateTime(2023, 1, 15),
                EndDate = new DateTime(2025, 1, 15),
                Status = ContractStatus.Active,
                ServiceLevel = "Premium Support - 24/7 Coverage"
            },
            new Contract
            {
                ClientId = clients[1].Id,
                StartDate = new DateTime(2022, 6, 1),
                EndDate = new DateTime(2024, 6, 1),
                Status = ContractStatus.Expired,
                ServiceLevel = "Standard Support - Business Hours"
            },
            new Contract
            {
                ClientId = clients[2].Id,
                StartDate = new DateTime(2024, 3, 10),
                EndDate = new DateTime(2026, 3, 10),
                Status = ContractStatus.Active,
                ServiceLevel = "Enterprise SLA - 99.9% Uptime"
            },
            new Contract
            {
                ClientId = clients[3].Id,
                StartDate = new DateTime(2024, 9, 1),
                EndDate = new DateTime(2025, 9, 1),
                Status = ContractStatus.OnHold,
                ServiceLevel = "Basic Support Package"
            },
            new Contract
            {
                ClientId = clients[4].Id,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2026, 1, 1),
                Status = ContractStatus.Draft,
                ServiceLevel = "Custom Integration Services"
            }
        };

        context.Contracts.AddRange(contracts);
        await context.SaveChangesAsync();

        // Create 10 Service Requests (only for Active and Draft contracts per business rules)
        var serviceRequests = new List<ServiceRequest>
        {
            new ServiceRequest
            {
                ContractId = contracts[0].Id, // Active
                Description = "Database performance optimization and query tuning",
                Cost = 2500.00m,
                LocalCost = 46250.00m, // Approximate ZAR conversion
                Status = "Completed"
            },
            new ServiceRequest
            {
                ContractId = contracts[0].Id, // Active
                Description = "Security audit and penetration testing",
                Cost = 5000.00m,
                LocalCost = 92500.00m,
                Status = "In Progress"
            },
            new ServiceRequest
            {
                ContractId = contracts[0].Id, // Active
                Description = "Cloud infrastructure migration consultation",
                Cost = 7500.00m,
                LocalCost = 138750.00m,
                Status = "Pending"
            },
            new ServiceRequest
            {
                ContractId = contracts[2].Id, // Active
                Description = "API integration for payment gateway",
                Cost = 3200.00m,
                LocalCost = 59200.00m,
                Status = "Completed"
            },
            new ServiceRequest
            {
                ContractId = contracts[2].Id, // Active
                Description = "Custom reporting dashboard development",
                Cost = 4800.00m,
                LocalCost = 88800.00m,
                Status = "In Progress"
            },
            new ServiceRequest
            {
                ContractId = contracts[2].Id, // Active
                Description = "Load testing and performance benchmarking",
                Cost = 1800.00m,
                LocalCost = 33300.00m,
                Status = "Pending"
            },
            new ServiceRequest
            {
                ContractId = contracts[4].Id, // Draft
                Description = "Initial system architecture design",
                Cost = 6000.00m,
                LocalCost = 111000.00m,
                Status = "Pending"
            },
            new ServiceRequest
            {
                ContractId = contracts[4].Id, // Draft
                Description = "Requirements gathering and analysis",
                Cost = 2200.00m,
                LocalCost = 40700.00m,
                Status = "Pending"
            },
            new ServiceRequest
            {
                ContractId = contracts[0].Id, // Active
                Description = "24/7 monitoring setup and configuration",
                Cost = 1500.00m,
                LocalCost = 27750.00m,
                Status = "Completed"
            },
            new ServiceRequest
            {
                ContractId = contracts[2].Id, // Active
                Description = "Disaster recovery planning and implementation",
                Cost = 8500.00m,
                LocalCost = 157250.00m,
                Status = "In Progress"
            }
        };

        context.ServiceRequests.AddRange(serviceRequests);
        await context.SaveChangesAsync();
    }
}
