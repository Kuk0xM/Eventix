namespace Eventix.SharedKernel.Domain;

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleValidationException : Exception
{
    public IBusinessRule BrokenRule { get; }

    public string Details { get; }

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.Message)
    {
        BrokenRule = brokenRule;
        Details = brokenRule.Message;
    }

    public override string ToString() => $"{base.ToString()}. Details: {Details}";
}