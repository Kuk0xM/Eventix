namespace Eventix.SharedKernel.Domain;

/// <summary>
/// Contract for business rules in the domain.
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// Returns true if the rule is being violated.
    /// </summary>
    bool IsBroken();

    /// <summary>
    /// Descriptive message of why the rule is broken.
    /// </summary>
    string Message { get; }
}