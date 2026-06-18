# Migration Plan: Organization-Centric Architecture Implementation

**Start Date:** 2026-06-18  
**Target:** Implement architecture-specification.md  
**Approach:** 6 phases with build verification after each phase

---

## Executive Summary

This plan transforms the auth service from a simple multi-tenant model to an organization-centric architecture matching the specification. Key changes:

1. **Separate Identity from Tenancy**: ApplicationUser becomes identity-only (no direct TenantId/RoleId)
2. **Introduce Membership**: Connects users to organizations with roles
3. **Support Multiple Paths**: Personal registration (no org) + Business registration (creates org)
4. **Enable Multi-Org**: Users can join multiple organizations and switch between them
5. **Strengthen Authorization**: Membership → Role → Permissions chain

---

## Current State Analysis

### Existing Strengths ✅
- Base Entity and TenantEntity classes exist
- RefreshToken uses TokenHash (secure)
- Role/Permission/RolePermission entities well-designed
- Global query filters for tenant isolation already in place
- Repository pattern and base infrastructure exists
- Registration and authentication services functional

### Gap Analysis ❌
- **No Membership entity** - Cannot separate user from tenant relationship
- **No OrganizationInvitation entity** - Cannot handle invitations
- **ApplicationUser is tangled** - Has TenantId and RoleId directly
- **Tenant entity is minimal** - Missing OwnerId, Slug, Plan, Status, etc.
- **Single registration flow** - Doesn't distinguish personal vs business
- **JWT doesn't carry membership context** - Can't validate authorization properly
- **Authorization not membership-based** - Still using direct user role
- **No organization switching** - Can't support multi-org users

---

## Architecture Overview (Target State)

```
ApplicationUser (Identity - Email, Password, Profile)
    ↓
Membership (User participates in Organization with Role)
    ↓
Role (Permissions for organization context)
    ↓
Permission (What user can do in organization)

Tenant (Internal Implementation - Multi-Tenancy)
    ↓ (Aliased as)
Organization (Business Concept - Shown in API)
```

---

## Phase-by-Phase Implementation Plan

### Phase 1: Domain Model - Membership & Invitations
**Duration:** ~2 hours  
**Complexity:** Medium

**What's Added:**
- `Membership` entity (User ↔ Tenant + Role)
- `OrganizationInvitation` entity (Pending invitations)
- Expand `Tenant` entity with new fields

**What Changes:**
- AppDbContext: Add relationships, global query filters
- Create initial migration

**Verification:**
- ✓ Solution builds
- ✓ Existing auth still works
- ✓ New entities available in DbContext

---

### Phase 2: Registration Flows
**Duration:** ~3 hours  
**Complexity:** High

**What's Added:**
- Personal registration: User only, no organization
- Business registration: User + Organization + Membership

**What Changes:**
- `AuthenticationService.RegisterAsync()` - Split into two flows
- `RegisterRequest` - Add `isBusinessRegistration` flag
- Create `MembershipRepository` and `OrganizationService`

**Migration Needed:**
- Data migration: Create Membership entries for existing users
- Map existing direct TenantId/RoleId relationships to Membership

**Verification:**
- ✓ Personal reg creates user without tenant
- ✓ Business reg creates user + org + membership
- ✓ Existing users migrated to Membership
- ✓ JWT includes membership context
- ✓ Solution builds

---

### Phase 3: Invitations System
**Duration:** ~2 hours  
**Complexity:** Medium

**What's Added:**
- `InvitationService` - Create and manage invitations
- `OrganizationInvitationRepository` - Persistence layer
- Endpoints: Invite, Accept, List Pending

**What Changes:**
- `AuthController` - Add invitation endpoints
- Email sending (if needed for invitations)

**Verification:**
- ✓ Create invitation works
- ✓ Accept invitation for new user works
- ✓ Accept invitation for existing user works
- ✓ Solution builds

---

### Phase 4: Authorization Refactor
**Duration:** ~3 hours  
**Complexity:** High - Critical Security Phase

**What Changes:**
- JWT generation includes `membership_id` instead of direct `tenant_id` header
- `TenantContextMiddleware` extracts tenant from authenticated membership
- Authorization policies validate through membership chain
- Tenant context is derived from server, not client

**JWT Before:**
```json
{
  "sub": "user-123",
  "tenant_id": "from-header",  // CLIENT SUPPLIED - UNSAFE
  "role": "Admin"
}
```

**JWT After:**
```json
{
  "sub": "user-123",
  "membership_id": "mem-456",  // SERVER SIGNED
  "tenant_id": "tenant-789",   // DERIVED FROM MEMBERSHIP
  "role": "Admin"
}
```

**Authorization Chain:**
1. Validate JWT signature
2. Extract membership_id
3. Load membership (verify active, not revoked)
4. Load user (verify active)
5. Load tenant (verify active)
6. Load role and permissions
7. Evaluate policy

**Verification:**
- ✓ Old tokens stop working (expected)
- ✓ New tokens work correctly
- ✓ Tenant isolation maintained
- ✓ Permission checks work
- ✓ Solution builds

---

### Phase 5: Onboarding UI Components
**Duration:** ~2 hours  
**Complexity:** Medium

**What's Added:**
- Organization Controller with management endpoints
- DTOs for organization operations
- Endpoints for wizard and member management

**Endpoints Added:**
- POST `/api/organizations` - Create organization
- GET `/api/organizations/{id}` - Get org details
- PUT `/api/organizations/{id}` - Update org
- POST `/api/organizations/{id}/members` - List members
- POST `/api/organizations/{id}/invitations` - Send invite
- GET `/api/organizations/{id}/invitations/pending` - List pending

**Verification:**
- ✓ Organization creation works
- ✓ Member listing works
- ✓ Pending invitations shown
- ✓ Solution builds

---

### Phase 6: Multi-Organization Support
**Duration:** ~2 hours  
**Complexity:** Medium

**What's Added:**
- Switch organization endpoint
- List user's organizations endpoint
- Active membership tracking

**Endpoints Added:**
- POST `/api/auth/switch-organization` - Change active org
- GET `/api/auth/my-organizations` - List all user's orgs

**Logic:**
- Validate user owns membership
- Generate new JWT with new membership context
- Return new token to client

**Verification:**
- ✓ Switch organization works
- ✓ New JWT has correct membership_id
- ✓ Tenant context switches
- ✓ All existing flows still work
- ✓ Solution builds

---

## Files to Create (Summary)

**Phase 1:**
- `Auth.Domain/Entities/Membership.cs`
- `Auth.Domain/Entities/OrganizationInvitation.cs`

**Phase 2:**
- `Auth.Infrastructure/Repositories/IMembershipRepository.cs`
- `Auth.Infrastructure/Repositories/MembershipRepository.cs`
- `Auth.Infrastructure/Services/IOrganizationService.cs`
- `Auth.Infrastructure/Services/OrganizationService.cs`

**Phase 3:**
- `Auth.Infrastructure/Repositories/IOrganizationInvitationRepository.cs`
- `Auth.Infrastructure/Repositories/OrganizationInvitationRepository.cs`
- `Auth.Infrastructure/Services/IInvitationService.cs`
- `Auth.Infrastructure/Services/InvitationService.cs`
- `Auth.Application/DTOs/CreateInvitationRequest.cs`
- `Auth.Application/DTOs/AcceptInvitationRequest.cs`

**Phase 5:**
- `AuthApi/Controllers/OrganizationController.cs`
- `Auth.Application/DTOs/OrganizationDetailsRequest.cs`
- `Auth.Application/DTOs/OrganizationDto.cs`
- `Auth.Application/DTOs/MembershipDto.cs`
- `Auth.Application/DTOs/InvitationDto.cs`

**Phase 6:**
- `Auth.Application/DTOs/SwitchOrganizationRequest.cs`
- `Auth.Application/DTOs/OrganizationListResponse.cs`

---

## Files to Modify (Summary)

**Phase 1:**
- `Auth.Domain/Entities/Tenant.cs` - Add fields
- `Auth.Infrastructure/Persistence/AppDbContext.cs` - Configure entities, add DbSet

**Phase 2:**
- `Auth.Application/DTOs/RegisterRequest.cs`
- `Auth.Infrastructure/Services/AuthenticationService.cs`
- `Program.cs` - DI registration

**Phase 3:**
- `AuthApi/Controllers/AuthController.cs` - Add endpoints
- `Program.cs` - DI registration

**Phase 4:**
- `Auth.Application/Services/IJwtTokenGenerator.cs`
- `Auth.Application/Services/JwtTokenGenerator.cs`
- `Auth.Infrastructure/Services/AuthenticationService.cs` - Token generation
- `AuthApi/Middleware/TenantContextMiddleware.cs`
- `AuthApi/Authorization/AuthorizationPolicies.cs`

**Phase 5:**
- No core modifications, just new endpoints

**Phase 6:**
- `Auth.Infrastructure/Repositories/IMembershipRepository.cs` - Add method
- `Auth.Infrastructure/Repositories/MembershipRepository.cs` - Add method
- `Auth.Infrastructure/Services/AuthenticationService.cs` - Add method

---

## Build Verification After Each Phase

```bash
# Check compilation
dotnet build

# Run migrations (if new entities added)
dotnet ef migrations add PhaseX_Description
dotnet ef database update

# Verify endpoints (manual testing or integration tests)
# Verify existing auth flows still work
```

---

## Risk Mitigation

### Risk: Breaking changes affect existing users
**Mitigation:** 
- Data migration script for existing users → Membership
- Support both old and new JWT formats temporarily
- Rollback plan: Keep TenantId/RoleId on ApplicationUser during transition

### Risk: Authorization becomes complex
**Mitigation:**
- Clear authorization layer between HTTP and business logic
- Comprehensive tests for permission checks
- Logging of authorization decisions

### Risk: Performance degradation with more entities
**Mitigation:**
- Use DbContext.Include() to load related entities efficiently
- Add indexes on foreign keys (automatic via EF)
- Query optimization during Phase 4

### Risk: Migration data loss
**Mitigation:**
- Full backup before migration
- Verify migration scripts on staging first
- Data validation checks before and after

---

## Success Criteria

After all phases complete:

1. ✓ Users can register without creating an organization
2. ✓ Business users can register and organization created automatically
3. ✓ Creator receives Owner role on organization
4. ✓ Users can be invited via email
5. ✓ Memberships stored independently from users
6. ✓ Authorization resolves through Membership → Role → Permission
7. ✓ JWTs contain membership context (membership_id, tenant_id)
8. ✓ Tenant context resolved from authenticated membership
9. ✓ Organization switching supported
10. ✓ Cross-tenant access prevented by design
11. ✓ Solution builds successfully ✓
12. ✓ All existing endpoints work ✓
13. ✓ All existing auth flows work ✓

---

## Timeline Estimate

| Phase | Est. Duration | Complexity | Risk |
|-------|---------------|-----------|------|
| 1. Domain Model | 2h | Medium | Low |
| 2. Registration | 3h | High | Medium |
| 3. Invitations | 2h | Medium | Low |
| 4. Authorization | 3h | High | **High** |
| 5. Onboarding | 2h | Medium | Low |
| 6. Multi-Org | 2h | Medium | Low |
| **Total** | **~14 hours** | - | - |

---

## Next Steps

1. ✓ Analysis completed
2. ✓ Migration plan created
3. → **Start Phase 1: Domain Model**

---

## Appendix: Architecture Specification Reference

From `architecture-specification.md`:
- Multi-tenant with organization ownership
- Identity separated from tenancy
- Membership-based authorization
- Support for personal and business registration
- Invitation system for team building
- Future: Multi-organization membership per user
- Security: JWT-based with membership context

