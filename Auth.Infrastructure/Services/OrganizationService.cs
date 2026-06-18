using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Auth.Infrastructure.Services;

public class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMembershipRepository _membershipRepository;

    public OrganizationService(
        AppDbContext dbContext,
        ITenantRepository tenantRepository,
        IMembershipRepository membershipRepository)
    {
        _dbContext = dbContext;
        _tenantRepository = tenantRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<Tenant> CreateOrganizationAsync(
        string name,
        string slug,
        string ownerId,
        string tenantType = "Organization")
    {
        // Verify slug is unique
        if (!await IsSlugUniqueAsync(slug))
        {
            throw new InvalidOperationException($"Organization slug '{slug}' is already in use.");
        }

        var organization = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            OwnerId = ownerId,
            Type = tenantType == "Organization" ? TenantType.Organization : TenantType.Individual,
            Status = "Active",
            Plan = "Free",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _tenantRepository.AddAsync(organization);
        await _tenantRepository.SaveChangesAsync();

        return organization;
    }

    public async Task<Membership> CreateMembershipAsync(
        string userId,
        Guid tenantId,
        Guid roleId,
        string? invitedBy = null)
    {
        // Verify tenant exists
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Organization with ID '{tenantId}' not found.");
        }

        // Check if membership already exists
        var existingMembership = await _membershipRepository.GetMembershipAsync(userId, tenantId);
        if (existingMembership != null)
        {
            throw new InvalidOperationException($"User is already a member of this organization.");
        }

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            RoleId = roleId,
            Status = "Active",
            JoinedAt = DateTime.UtcNow,
            InvitedBy = invitedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _membershipRepository.CreateMembershipAsync(membership);

        return membership;
    }

    public async Task<Tenant?> GetOrganizationAsync(Guid id)
    {
        return await _tenantRepository.GetByIdAsync(id);
    }

    public async Task<Tenant?> GetOrganizationBySlugAsync(string slug)
    {
        return await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug)
    {
        return !await _dbContext.Tenants
            .AnyAsync(t => t.Slug == slug);
    }

    public async Task<string> GenerateUniqueSlugAsync(string organizationName)
    {
        var baseSlug = ConvertToSlug(organizationName);
        var slug = baseSlug;
        var counter = 1;

        while (!await IsSlugUniqueAsync(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    public async Task<IEnumerable<Tenant>> GetUserOwnedOrganizationsAsync(string userId)
    {
        return await _dbContext.Tenants
            .Where(t => t.OwnerId == userId && t.IsActive)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Converts a name to a URL-safe slug.
    /// </summary>
    private static string ConvertToSlug(string name)
    {
        // Convert to lowercase
        var slug = name.ToLowerInvariant();

        // Remove accents and special characters, keep only alphanumeric and hyphens
        slug = Regex.Replace(slug, @"[^\w\s-]", "");

        // Replace whitespace with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Replace multiple hyphens with single hyphen
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // Limit to 50 characters
        if (slug.Length > 50)
        {
            slug = slug.Substring(0, 50).TrimEnd('-');
        }

        return slug;
    }
}
