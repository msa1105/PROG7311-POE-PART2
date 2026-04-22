using PROG7311_POE.Models;
using PROG7311_POE.Patterns.Factory;

namespace GLMS.Tests;

/// <summary>
/// Tests for the Factory Pattern (Contract Creation).
/// Verifies that GlmsContractFactory creates the correct concrete types,
/// validates them, and rejects bad input.
/// </summary>
public class ContractFactoryTests
{
    private readonly GlmsContractFactory _factory = new();

    [Fact]
    public void CreateContract_Domestic_ReturnsDomesticContract()
    {
        var contract = _factory.CreateContract("domestic");
        Assert.IsType<DomesticContract>(contract);
    }

    [Fact]
    public void CreateContract_International_ReturnsInternationalContract()
    {
        var contract = _factory.CreateContract("international");
        Assert.IsType<InternationalContract>(contract);
    }

    [Fact]
    public void CreateContract_UnknownType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _factory.CreateContract("freight"));
    }

    [Fact]
    public void DomesticContract_Validate_PassesWithValidData()
    {
        var contract = new DomesticContract
        {
            ContractId = 1,
            Region = "Gauteng",
            LocalRate = 750m,
            Status = ContractStatus.Active
        };
        Assert.True(contract.Validate());
    }

    [Fact]
    public void DomesticContract_Validate_FailsWithMissingRegion()
    {
        var contract = new DomesticContract { ContractId = 1, LocalRate = 500m };
        Assert.Throws<InvalidOperationException>(() => contract.Validate());
    }

    [Fact]
    public void InternationalContract_Validate_PassesWithValidData()
    {
        var contract = new InternationalContract
        {
            ContractId = 2,
            OriginCountry = "ZA",
            DestinationCountry = "US",
            CurrencyCode = "USD"
        };
        Assert.True(contract.Validate());
    }

    [Fact]
    public void InternationalContract_Validate_FailsWithMissingCurrencyCode()
    {
        var contract = new InternationalContract
        {
            ContractId = 2,
            OriginCountry = "ZA",
            DestinationCountry = "US"
        };
        Assert.Throws<InvalidOperationException>(() => contract.Validate());
    }

    [Fact]
    public void RegisterContract_ValidContract_AddsToRegistry()
    {
        var contract = new DomesticContract
        {
            ContractId = 3,
            Region = "Western Cape",
            LocalRate = 600m
        };
        _factory.RegisterContract(contract);
        Assert.Contains(contract, _factory.GetRegisteredContracts());
    }

    [Fact]
    public void RegisterContract_InvalidContract_ThrowsAndDoesNotRegister()
    {
        var contract = new DomesticContract { ContractId = 4 }; // missing Region and LocalRate
        Assert.Throws<InvalidOperationException>(() => _factory.RegisterContract(contract));
        Assert.DoesNotContain(contract, _factory.GetRegisteredContracts());
    }

    [Fact]
    public void DomesticContract_GetDetails_ContainsContractId()
    {
        var contract = new DomesticContract { ContractId = 5, Region = "Limpopo", LocalRate = 400m };
        Assert.Contains("5", contract.GetDetails());
    }
}
