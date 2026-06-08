//Code attribution
//Refactoring.Guru. 2024. Factory Method Design Pattern.
//Available at: https://refactoring.guru/design-patterns/factory-method
//[Accessed: 22 April 2025]

using PROG7311_POE.Models;

namespace PROG7311_POE.Patterns.Factory;

/// <summary>
/// Abstract Creator — Contract Factory.
/// Defines the factory interface for creating and registering contract instances.
/// Subclass or use directly with the factory method to produce IContract objects.
/// </summary>
public abstract class ContractFactory
{
    private readonly List<IContract> _registry = new();

    /// <summary>
    /// Factory method: creates an IContract of the specified type.
    /// </summary>
    /// <param name="type">"Domestic" or "International"</param>
    public abstract IContract CreateContract(string type);

    /// <summary>
    /// Validates and adds the contract to the internal registry.
    /// Throws if contract validation fails.
    /// </summary>
    public void RegisterContract(IContract contract)
    {
        if (contract == null) throw new ArgumentNullException(nameof(contract));
        contract.Validate(); // throws InvalidOperationException on bad data
        _registry.Add(contract);
    }

    /// <summary>Returns a read-only view of all registered contracts.</summary>
    public IReadOnlyList<IContract> GetRegisteredContracts() => _registry.AsReadOnly();
}

/// <summary>
/// Concrete Creator — GLMS Contract Factory.
/// Instantiates DomesticContract or InternationalContract based on the type string.
/// </summary>
public class GlmsContractFactory : ContractFactory
{
    public override IContract CreateContract(string type) => type?.ToLowerInvariant() switch
    {
        "domestic"      => new DomesticContract(),
        "international" => new InternationalContract(),
        _               => throw new ArgumentException($"Unknown contract type '{type}'. Use 'Domestic' or 'International'.")
    };
}
