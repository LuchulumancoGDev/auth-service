# Architecture Analysis: Clean Architecture & Tenant Isolation Review

**Date:** 2026-05-08  
**Status:** Initial codebase review - early stage project

---

## 1. CLEAN ARCHITECTURE VIOLATIONS

### 1.1 Domain Layer Contamination - CRITICAL

**Issue:** `ApplicationUser` inherits from `IdentityUser` (Infrastructure concern)

```csharp
// ❌ VIOLATES: Domain should be framework-independent
public class ApplicationUser: IdentityUser
```

**Why this matters:**
- Domain entity directly depends on ASP.NET Core Identity
- Cannot test domain logic without infrastructure
- Couples business rules to Microsoft's implementation choices
- Violates Dependency Inversion Principle

**Impact:** Must refactor to custom User entity; IdentityUser only in Infrastructure

---

### 1.2 Missing Base Entity Abstractions

**Issue:** No `Entity` or `TenantEntity` base classes per spec

```csharp
// ❌ MISSING: Per CLAUDE.md requirements
public abstract class Entity { ... }
public abstract class TenantEntity : Entity { ... }
```

**Current state:**
- `Tenant` has minimal fields; missing audit metadata
- `Permission`, `RolePermission`, `UserPermission` lack consistency
- No `CreatedAt`, `UpdatedAt`, `CreatedBy` fields

**Impact:** Cannot implement audit trails; inconsistent entity lifecycle management

---

### 1.3 Plaintext Refresh Token Storage - CRITICAL SECURITY ISSUE

**Issue:** `RefreshToken.Token` stored unencrypted

```csharp
public class RefreshToken
{
    public string Token { get; set; }  // ❌ Stored plaintext!
}
```

**Spec requirement:** "Refresh tokens must be hashed before storage. Never stored plaintext."

**Impact:**
- Database breach exposes all valid refresh tokens
- Tokens become usable immediately by attacker
- Violates security requirements

---

### 1.4 Multi-Role System Not Aligned to Spec

**Issue:** Supports arbitrary user-permission assignment; contradicts "One User = One Role"

```csharp
public class UserPermission
{
    public string UserId { get; set; }
    public Permission Permission { get; set; }  // ❌ Allows direct permission assignment
}
```

**Spec requirement:** "Each user has exactly ONE role. Users must NOT create/modify roles."

**Current problem:**
- `UserPermission` table allows bypassing role constraints
- User could have direct permissions + role permissions = complexity
- Not prevented by schema

**Impact:** Authorization becomes unpredictable; violates core architecture principle

---

### 1.5 String-Based Type Fields Instead of Enums

**Issue:** AccountType and UserType stored as strings

```csharp
public string AccountType { get; set; }  // ❌ Should be enum
public string UserType { get; set; }     // ❌ Should be enum
```

**Impact:** No compile-time safety; invalid values possible; queries become fragile

---

### 1.6 Missing Custom Role Entity

**Issue:** `RolePermission` uses `IdentityRole` directly

```csharp
public IdentityRole Role { get; set; }  // ❌ Infrastructure leak
```

**Spec requirement:** "Roles are Admin, Driver, Customer - seeded, immutable, system-defined"

**Current problem:**
- No custom Role entity in Domain
- Cannot add role-specific behavior
- Cannot seed properly

---

### 1.7 Mandatory Repositories Missing

**Spec requirement:** Required repositories include UserRepository, TenantRepository, RoleRepository, PermissionRepository, RefreshTokenRepository

**Current state:** No repositories implemented yet

---

## 2. TENANT ISOLATION RISKS

### 2.1 No Global Query Filters - CRITICAL

**Issue:** DbContext has NO global query filters for tenant isolation

```csharp
// ❌ MISSING: Per CLAUDE.md
// modelBuilder.Entity<ApplicationUser>().HasQueryFilter(...)
// Global filters should auto-filter by TenantId
```

**Current state:**
- Tenant filtering must be manually applied in every query
- Easy to forget; leads to data leaks
- Cannot be enforced by architecture

**Risk:** Cross-tenant data access through developer oversight

---

### 2.2 Nullable TenantId on ApplicationUser

**Issue:** `TenantId` is nullable

```csharp
public Guid? TenantId { get; set; }  // ❌ Should be required (NOT NULL)
```

**Problem:**
- Orphaned users could exist without tenant
- Violates tenant isolation assumptions
- Queries must handle null case

**Risk:** Orphaned users bypass tenant filtering

---

### 2.3 RefreshToken Lacks TenantId

**Issue:** `RefreshToken` has no `TenantId` field

```csharp
public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public string UserId { get; set; }
    // ❌ MISSING: public Guid TenantId { get; set; }
}
```

**Risk:** Cannot filter refresh tokens by tenant; reuse across tenants possible

---

### 2.4 Permission Model Lacks Tenant Scope

**Issue:** `Permission` and `RolePermission` are global; not per-tenant

```csharp
public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    // ❌ MISSING: public Guid TenantId { get; set; }
}
```

**Problem:**
- All tenants share same permission set
- Cannot define custom permissions per tenant
- No tenant isolation on permission checks

---

### 2.5 No Tenant Context Provider

**Issue:** No service to provide current authenticated tenant

**Current state:**
- No `ICurrentTenantProvider` or similar
- No middleware to extract tenant from JWT

**Risk:** Application doesn't know which tenant is requesting

---

### 2.6 TenantId Never Validated from JWT

**Spec requirement:** "Never trust TenantId from client requests. Tenant context must come from JWT claims or authenticated user context."

**Current state:** No validation layer exists yet

---

### 2.7 UserPermission Lacks TenantId

**Issue:** No tenant scoping on user-level permissions

```csharp
public class UserPermission
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public Guid PermissionId { get; set; }
    // ❌ MISSING: public Guid TenantId { get; set; }
}
```

---

## 3. IMPLEMENTATION PRIORITIES

### Tier 1: CRITICAL (Security & Core Isolation)

1. **Implement RefreshToken hashing** - Prevent plaintext tokens in database
2. **Create global query filters** - Enforce automatic tenant isolation
3. **Make TenantId required** - Eliminate orphaned users
4. **Refactor ApplicationUser** - Extract from IdentityUser dependency

### Tier 2: IMPORTANT (Architecture Integrity)

5. **Create Role entity** - Implement one-user-one-role constraint
6. **Create base entities** - Entity, TenantEntity, AuditableEntity
7. **Create tenant context provider** - Service to provide current tenant
8. **Add TenantId to RefreshToken** - Enforce token isolation

### Tier 3: DESIGN (User Type Safety & Seeding)

9. **Convert AccountType/UserType to enums** - Type safety
10. **Implement role seeding** - Immutable Admin/Driver/Customer roles
11. **Remove UserPermission** - Align to one-user-one-role principle
12. **Implement repositories** - Clean data access layer

---

## 4. REMEDIATION ROADMAP

```
Phase 1: Security & Isolation (Days 1-2)
├── Add RefreshToken hashing
├── Add global query filters
├── Make TenantId required
└── Add TenantId to RefreshToken

Phase 2: Architecture (Days 3-4)
├── Create base entity classes
├── Create custom Role entity
├── Create ApplicationUser replacement
└── Create repositories

Phase 3: Type Safety (Days 5-6)
├── Create enums for AccountType/UserType
├── Implement role seeding
├── Remove UserPermission table
└── Create tenant context provider

Phase 4: Integration (Days 7+)
├── Create authentication middleware
├── Create authorization policies
├── Create application services
└── Create API controllers
```

---

## 5. FILES TO CREATE/MODIFY

### To Create:
- `Auth.Domain/Entities/Entity.cs` - Base entity class
- `Auth.Domain/Entities/TenantEntity.cs` - Tenant-scoped entity base
- `Auth.Domain/Entities/AuditableEntity.cs` - Audit trail support
- `Auth.Domain/Entities/Role.cs` - Custom role entity
- `Auth.Domain/Entities/User.cs` - Refactored user entity (no IdentityUser)
- `Auth.Domain/Enums/AccountType.cs` - Enum for account types
- `Auth.Domain/Enums/UserType.cs` - Enum for user types
- `Auth.Domain/ValueObjects/HashedRefreshToken.cs` - Secure token value object
- `Auth.Infrastructure/Repositories/IUserRepository.cs` - Repository interface
- `Auth.Infrastructure/Repositories/UserRepository.cs` - Repository implementation
- `Auth.Infrastructure/Services/CurrentTenantProvider.cs` - Tenant context service
- `Auth.Application/Services/TokenHashingService.cs` - Hash refresh tokens

### To Modify:
- `Auth.Infrastructure/Persistence/AppDbContext.cs` - Add global filters, update config
- `Auth.Domain/Entities/RefreshToken.cs` - Add TenantId, hash token
- `Auth.Domain/Entities/Permission.cs` - Add TenantId
- `Auth.Domain/Entities/RolePermission.cs` - Update to custom Role
- `Auth.Domain/Entities/ApplicationUser.cs` - Remove IdentityUser inheritance
- `Auth.Infrastructure/Migrations/*` - Create new migration after changes

### To Remove:
- `Auth.Domain/Entities/UserPermission.cs` - Contradicts one-user-one-role
- Migration tables: `UserPermissions` table

