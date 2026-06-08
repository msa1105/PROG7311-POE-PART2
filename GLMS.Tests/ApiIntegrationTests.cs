using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PROG7311_POE.Data;
using PROG7311_POE.Models;

namespace GLMS.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", "InMemory");
        });
    }

    [Fact]
    public async Task GetContracts_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/Contracts");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Authenticate_WithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest { Email = "demo@glms.com", Password = "Password123!" };

        // Act
        var response = await client.PostAsJsonAsync("api/Account/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task GetClients_WithAuth_ReturnsSuccessAndClientsList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest { Email = "demo@glms.com", Password = "Password123!" };
        
        // Authenticate first
        var authResponse = await client.PostAsJsonAsync("api/Account/login", loginRequest);
        var authResult = await authResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(authResult);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);

        // Act
        var response = await client.GetAsync("api/Clients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var clients = await response.Content.ReadFromJsonAsync<List<Client>>();
        Assert.NotNull(clients);
    }

    [Fact]
    public async Task GetContracts_WithAuth_ReturnsSuccessAndContractsList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest { Email = "demo@glms.com", Password = "Password123!" };
        
        // Authenticate first
        var authResponse = await client.PostAsJsonAsync("api/Account/login", loginRequest);
        var authResult = await authResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(authResult);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);

        // Act
        var response = await client.GetAsync("api/Contracts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
        Assert.NotNull(contracts);
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
