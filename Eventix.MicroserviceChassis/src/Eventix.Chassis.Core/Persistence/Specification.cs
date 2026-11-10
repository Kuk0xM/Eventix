using System.Linq.Expressions;

namespace Eventix.Chassis.Core.Persistence;

/// <summary>
/// Base class for building specifications with a fluent, protected API.
/// Derived specs configure themselves in their constructor.
/// </summary>
public abstract class Specification<T> : ISpecification<T> where T : class
{
    private readonly List<Expression<Func<T, object>>> _includes = new();

    public Expression<Func<T, bool>>? Criteria { get; protected set; }

    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes;

    public Expression<Func<T, object>>? OrderBy { get; protected set; }

    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }

    public int? Take { get; protected set; }

    public int? Skip { get; protected set; }

    /// <summary>
    /// Adds an eager-load navigation (EF Include) to the specification.
    /// </summary>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        _includes.Add(includeExpression);
    }
}