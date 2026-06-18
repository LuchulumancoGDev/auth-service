using System;
using System.Threading.Tasks;
using Xunit;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Services;
using Auth.Domain.Entities;
using Auth.Application.DTOs;
using System.Linq;

namespace Auth.Tests;

public class FakeEmailSender : IEmailSender
{
    public string? LastTo;
    public string? LastToken;
    public Guid? LastTenant;
    public string? LastInvitedBy;

    public Task SendInvitationAsync(string toEmail, string token, Guid tenantId, string? invitedBy)
    {
        LastTo = toEmail;
        LastToken = token;
        LastTenant = tenantId;
        LastInvitedBy = invitedBy;
        return Task.CompletedTask;
    }
}

public class InvitationServiceTests
{
    [Fact]
    public async Task CreateInvitation_CreatesRecordAndSendsEmail()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        // seed a tenant, role and a user
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Acme" , CreatedAt = DateTime.UtcNow};
        context.Tenants.Add(tenant);

        var role = new Role { Id = Guid.NewGuid(), Name = "Member", CreatedAt = DateTime.UtcNow };
        context.Roles.Add(role);

        var user = new ApplicationUser { Id = "user-1", Email = "owner@example.com", UserName = "owner@example.com", TenantId = tenant.Id, CreatedAt = DateTime.UtcNow };
        context.Users.Add(user);

        await context.SaveChangesAsync();

        var roleRepo = new RoleRepository(context);
        var membershipRepo = new MembershipRepository(context);
        var invitationRepo = new OrganizationInvitationRepository(context);
        var userRepo = new UserRepository(context);

        var emailSender = new FakeEmailSender();
        var svc = new InvitationService(invitationRepo, membershipRepo, roleRepo, userRepo, emailSender);

        var req = new CreateInvitationRequest { Email = "invitee@example.com", RoleName = "Member" };
        var result = await svc.CreateInvitationAsync(user.Id, tenant.Id, req);

        Assert.NotNull(result);
        Assert.Equal(req.Email, result.Email);
        Assert.Equal("Member", result.RoleName);
        Assert.False(string.IsNullOrEmpty(result.Token));

        // verify invitation persisted
        var persisted = context.OrganizationInvitations.FirstOrDefault(i => i.Id == result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(req.Email, persisted.Email);

        // verify email sent
        Assert.Equal("invitee@example.com", emailSender.LastTo);
        Assert.Equal(result.Token, emailSender.LastToken);
    }
}
