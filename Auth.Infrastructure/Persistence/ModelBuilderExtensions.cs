using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Auth.Infrastructure.Persistence;

public static class ModelBuilderExtensions
{
    // Call from AppDbContext.OnModelCreating(this, context) passing the DbContext instance.
    // This builds filters of the form: e => !context.CurrentTenantId.HasValue || e.TenantId == context.CurrentTenantId
    public static void ApplyTenantQueryFilters(this ModelBuilder builder, DbContext context)
    {
        var currentTenantProperty = Expression.Property(Expression.Constant(context), nameof(AppDbContext.ContextId));

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var tenantProp = clrType.GetProperty("TenantId");
            if (tenantProp == null) continue;

            // parameter: e
            var parameter = Expression.Parameter(clrType, "e");
            // e.TenantId
            var tenantProperty = Expression.Property(parameter, tenantProp);

            // !context.CurrentTenantId.HasValue
            var hasValue = Expression.Property(currentTenantProperty, "HasValue");
            var notHasValue = Expression.IsFalse(hasValue);

            // e.TenantId == context.CurrentTenantId  (ensure types match)
            Expression right = currentTenantProperty;
            if (tenantProp.PropertyType == typeof(Guid))
            {
                right = Expression.Convert(currentTenantProperty, typeof(Guid));
            }
            else if (tenantProp.PropertyType == typeof(Guid?))
            {
                // already Guid?
            }

            var equals = Expression.Equal(tenantProperty, right);

            var body = Expression.OrElse(notHasValue, equals);
            var lambda = Expression.Lambda(body, parameter);

            builder.Entity(clrType).HasQueryFilter(lambda);
        }
    }
}