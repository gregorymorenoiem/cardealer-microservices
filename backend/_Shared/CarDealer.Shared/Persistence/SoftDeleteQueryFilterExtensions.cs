using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarDealer.Shared.Persistence;

/// <summary>
/// Extensions for applying global query filters for soft-delete entities.
/// Automatically filters out entities where IsDeleted == true.
/// </summary>
public static class SoftDeleteQueryFilterExtensions
{
    /// <summary>
    /// Applies HasQueryFilter(e => !e.IsDeleted) to all entities implementing ISoftDeletableEntity.
    /// Call this in OnModelCreating after base.OnModelCreating.
    /// 
    /// NOTE: If the entity also has a tenant filter from MultiTenantDbContext,
    /// the filters are combined with AND automatically by EF Core.
    /// </summary>
    public static ModelBuilder ApplySoftDeleteFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            // Skip derived types in TPH to avoid duplicate filters
            if (entityType.BaseType != null &&
                typeof(ISoftDeletableEntity).IsAssignableFrom(entityType.BaseType.ClrType))
                continue;

            var method = typeof(SoftDeleteQueryFilterExtensions)
                .GetMethod(nameof(ApplyFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(null, new object[] { modelBuilder });
        }

        return modelBuilder;
    }

    private static void ApplyFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletableEntity
    {
        // Check if entity already has a query filter (from multi-tenancy, etc.)
        var existingFilter = modelBuilder.Model
            .FindEntityType(typeof(TEntity))?
            .GetQueryFilter();

        if (existingFilter != null)
        {
            // Combine with existing filter using AND
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDeletableEntity.IsDeleted));
            var notDeleted = Expression.Not(isDeletedProperty);

            // Re-bind existing filter's parameter
            var existingBody = ReplacingExpressionVisitor
                .Replace(existingFilter.Parameters[0], parameter, existingFilter.Body);

            var combinedBody = Expression.AndAlso(existingBody, notDeleted);
            var combinedFilter = Expression.Lambda<Func<TEntity, bool>>(combinedBody, parameter);

            modelBuilder.Entity<TEntity>().HasQueryFilter(combinedFilter);
        }
        else
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }

        // Add index on IsDeleted for better query performance
        modelBuilder.Entity<TEntity>()
            .HasIndex(e => e.IsDeleted)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_IsDeleted");
    }

    /// <summary>
    /// Visitor that replaces one expression with another within an expression tree.
    /// </summary>
    private class ReplacingExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        private ReplacingExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public static Expression Replace(Expression oldValue, Expression newValue, Expression expression)
        {
            return new ReplacingExpressionVisitor(oldValue, newValue).Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldValue ? _newValue : base.VisitParameter(node);
        }
    }
}
