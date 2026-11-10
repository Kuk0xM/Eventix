using Eventix.Chassis.Core.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Eventix.Chassis.Infrastructure.Persistence;

/// <summary>
/// Extension methods to apply ISpecification&lt;T&gt; to EF Core queries.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Applies a specification to a DbSet or IQueryable, composing
    /// WHERE, INCLUDE, ORDER BY, SKIP and TAKE clauses.
    /// 
    /// Usage:
    /// <code>
    /// var spec = new ActiveReservationsByEventSpec(eventId);
    /// var results = await dbContext.Reservations
    ///     .ApplySpecification(spec)
    ///     .ToListAsync();
    /// </code>
    /// </summary>
    public static IQueryable<T> ApplySpecification<T>(
        this IQueryable<T> query,
        ISpecification<T> specification) where T : class
    {
        // 1. Apply WHERE clause
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // 2. Apply eager-loading (Include)
        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        // 3. Apply ORDER BY
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // 4. Apply pagination (Skip + Take)
        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        return query;
    }
}