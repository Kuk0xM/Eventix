using System.Linq.Expressions;

namespace Eventix.Chassis.Core.Persistence;

/// <summary>
/// Specification Pattern: encapsulates a reusable query definition.
/// 
/// Instead of wrapping EF Core in a generic repository, we encapsulate
/// COMPLEX, REUSABLE queries as specifications. Simple CRUD goes directly
/// through the DbContext.
/// 
/// Example:
/// <code>
/// public class ActiveReservationsByEventSpec : Specification&lt;Reservation&gt;
/// {
///     public ActiveReservationsByEventSpec(Guid eventId)
///     {
///         Criteria = r => r.EventId == eventId &amp;&amp; r.Status == ReservationStatus.Confirmed;
///         AddInclude(r => r.Tickets);
///         OrderBy = r => r.CreatedAt;
///     }
/// }
/// </code>
/// </summary>
public interface ISpecification<T> where T : class
{
    /// <summary>
    /// The WHERE clause of the query.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Navigation properties to eager-load (EF Include).
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// ORDER BY ascending expression.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// ORDER BY descending expression.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Maximum number of results (TOP N).
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Number of results to skip (pagination).
    /// </summary>
    int? Skip { get; }
}