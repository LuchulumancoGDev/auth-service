# Multi-Tenant Organization Architecture Specification

## Document Information

| Field                | Value                                          |
| -------------------- | ---------------------------------------------- |
| Document Type        | Architecture Specification                     |
| System               | Multi-Tenant SaaS Platform                     |
| Version              | 1.0                                            |
| Status               | Proposed                                       |
| Architecture Pattern | Organization-Based Multi-Tenancy               |
| Inspired By          | GitHub, Slack, Microsoft 365, Google Workspace |

---

# 1. Purpose

This specification defines the identity, tenancy, authorization, onboarding, and organization management architecture for the platform.

The architecture must support:

* Individual users
* Business organizations
* Multi-tenant isolation
* Role-based access control
* Organization ownership
* Team invitations
* Future multi-organization membership
* Organization switching

The design separates user identity from organizational membership.

---

# 2. Architectural Principles

## 2.1 Identity and Tenancy Separation

User authentication and organization membership are separate concerns.

A user owns an identity.

Organizations own resources.

Membership grants access to organization resources.

---

## 2.2 Tenant Ownership

Every organization must have exactly one owner.

The owner is automatically assigned during organization creation.

Ownership may later be transferred.

---

## 2.3 Membership-Based Authorization

Permissions shall be evaluated using:

```text
User
    ↓
Membership
    ↓
Role
    ↓
Permissions
```

Permissions shall never be derived directly from the user record.

---

## 2.4 Future Multi-Organization Support

The architecture shall allow:

```text
User A
 ├── Organization A (Owner)
 ├── Organization B (Manager)
 └── Organization C (Customer)
```

without requiring multiple user accounts.

---

# 3. Domain Model

---

## 3.1 ApplicationUser

Represents a global authenticated identity.

### Responsibilities

* Authentication
* Profile management
* Password management
* Email verification

### Schema

```text
ApplicationUser
-------------------------
Id (GUID)
Email
PasswordHash
FirstName
LastName
PhoneNumber
EmailConfirmed
Status
CreatedAt
UpdatedAt
```

### Constraints

ApplicationUser shall NOT contain:

```text
TenantId
RoleId
```

---

## 3.2 Tenant

Represents an organization.

### Schema

```text
Tenant
-------------------------
Id (GUID)
Name
Slug
OwnerId
Plan
Status
Country
LogoUrl
CreatedAt
UpdatedAt
```

### Constraints

* Name must be unique within platform rules.
* Slug must be globally unique.
* Every tenant must have an owner.

---

## 3.3 Membership

Represents a user's participation in a tenant.

### Schema

```text
Membership
-------------------------
Id (GUID)
UserId
TenantId
RoleId
Status
JoinedAt
InvitedBy
CreatedAt
UpdatedAt
```

### Constraints

A membership must belong to:

```text
1 User
1 Tenant
1 Role
```

A user may have:

```text
0..N Memberships
```

---

## 3.4 Role

Represents organizational responsibility.

### Schema

```text
Role
-------------------------
Id
Name
Description
```

---

## 3.5 Permission

Represents an authorization capability.

### Schema

```text
Permission
-------------------------
Id
Name
Description
```

Examples:

```text
ViewOrders
CreateOrders
AssignDrivers
ManageCustomers
ManageBilling
ManageUsers
ManageOrganization
```

---

## 3.6 RolePermission

Maps permissions to roles.

### Schema

```text
RolePermission
-------------------------
RoleId
PermissionId
```

---

## 3.7 OrganizationInvitation

Represents a pending invitation.

### Schema

```text
OrganizationInvitation
-------------------------
Id
TenantId
Email
RoleId
Token
Status
ExpiresAt
AcceptedAt
InvitedBy
CreatedAt
```

---

# 4. Role Model

---

## 4.1 Platform Roles

Platform-wide roles are not tenant specific.

### Supported Roles

```text
PlatformAdmin
```

### Responsibilities

* Platform management
* Tenant oversight
* Support operations
* Billing administration

---

## 4.2 Organization Roles

Organization roles exist only inside a tenant.

### Standard Roles

```text
Owner
Admin
Manager
Dispatcher
Driver
Customer
```

---

## 4.3 Ownership Rules

The Owner role:

* Cannot be removed without ownership transfer
* Has all organization permissions
* May invite members
* May manage subscriptions
* May transfer ownership

---

# 5. Identity Architecture

---

## 5.1 Authentication

Authentication shall be provided through:

```text
JWT Bearer Tokens
```

Authentication identifies:

```text
Who the user is
```

---

## 5.2 Authorization

Authorization determines:

```text
What the user may do
```

Authorization must use membership context.

---

# 6. Tenant Context Resolution

---

## 6.1 Active Tenant

A user may have multiple memberships.

One membership is considered active.

The active membership determines:

```text
Tenant
Role
Permissions
```

for the current session.

---

## 6.2 JWT Claims

JWT tokens shall contain:

```text
sub
UserId

membership_id
MembershipId

tenant_id
TenantId

role
RoleName

email
Email
```

Example:

```json
{
  "sub": "user-123",
  "membership_id": "mem-456",
  "tenant_id": "tenant-789",
  "role": "Owner"
}
```

---

## 6.3 TenantContextMiddleware

### Responsibilities

Resolve:

```text
CurrentUser
CurrentTenant
CurrentMembership
CurrentRole
```

from the authenticated JWT.

### Security Rule

The system shall never trust a tenant identifier supplied by the client.

Tenant context must be derived from authenticated membership.

---

# 7. Registration Architecture

---

## 7.1 Personal Registration Flow

### Workflow

```text
Register User
    ↓
Create ApplicationUser
    ↓
Issue JWT
    ↓
Login
```

### Result

```text
User exists
No tenant exists
```

---

## 7.2 Business Registration Flow

### Workflow

```text
Register User
    ↓
Create ApplicationUser
    ↓
Create Tenant
    ↓
Create Membership
    ↓
Assign Owner Role
    ↓
Issue JWT
    ↓
Organization Setup
```

### Result

```text
Tenant Created
Membership Created
Owner Assigned
```

---

# 8. Organization Onboarding

---

## 8.1 Initial Setup Wizard

After business registration:

```text
Organization Details
    ↓
Business Name
Industry
Country
Logo
    ↓
Invite Team Members
    ↓
Dashboard
```

---

## 8.2 Required Data

### Organization Profile

```text
Business Name
Industry
Country
Logo
Timezone
```

---

# 9. Invitation Architecture

---

## 9.1 Invitation Creation

### Workflow

```text
Owner/Admin
    ↓
Invite User
    ↓
Generate Token
    ↓
Persist Invitation
    ↓
Send Email
```

---

## 9.2 Invitation Acceptance

### Existing User

```text
Accept Invitation
    ↓
Validate Token
    ↓
Create Membership
    ↓
Mark Invitation Accepted
```

---

## 9.3 New User

```text
Accept Invitation
    ↓
Register Account
    ↓
Create Membership
    ↓
Mark Invitation Accepted
```

---

# 10. Organization Switching

---

## 10.1 Objective

Support users belonging to multiple organizations.

---

## 10.2 Endpoint

```http
POST /api/auth/switch-organization
```

### Request

```json
{
  "membershipId": "membership-guid"
}
```

### Response

```json
{
  "token": "new-jwt-token"
}
```

---

## 10.3 Validation Rules

User must:

```text
Own Membership
Membership Active
Membership Not Revoked
```

---

# 11. Repository Specifications

---

## 11.1 Membership Repository

### Interface

```csharp
public interface IMembershipRepository
{
    Task<Membership?> GetMembership(Guid id);

    Task<IEnumerable<Membership>> GetUserMemberships(Guid userId);

    Task<IEnumerable<Membership>> GetTenantMembers(Guid tenantId);

    Task CreateMembership(Membership membership);

    Task RemoveMembership(Guid membershipId);

    Task UpdateRole(Guid membershipId, Guid roleId);

    Task<bool> IsOwner(Guid userId, Guid tenantId);

    Task<bool> IsMember(Guid userId, Guid tenantId);
}
```

---

## 11.2 Invitation Repository

### Interface

```csharp
public interface IOrganizationInvitationRepository
{
    Task CreateInvitation(
        OrganizationInvitation invitation);

    Task<OrganizationInvitation?> GetByToken(
        string token);

    Task<IEnumerable<OrganizationInvitation>>
        GetPending(Guid tenantId);

    Task Accept(Guid invitationId);

    Task Expire(Guid invitationId);

    Task Delete(Guid invitationId);
}
```

---

# 12. Database Architecture

## Logical Schema

```text
ApplicationUsers
-------------------------
Id (PK)
Email
PasswordHash
...

Tenants
-------------------------
Id (PK)
Name
Slug
OwnerId
Plan
Status

Memberships
-------------------------
Id (PK)
UserId (FK)
TenantId (FK)
RoleId (FK)
Status

Roles
-------------------------
Id (PK)
Name

Permissions
-------------------------
Id (PK)
Name

RolePermissions
-------------------------
RoleId (FK)
PermissionId (FK)

OrganizationInvitations
-------------------------
Id (PK)
TenantId (FK)
Email
RoleId (FK)
Token
Status
ExpiresAt
AcceptedAt
```

---

# 13. Security Requirements

---

## Authentication

* JWT Bearer Authentication
* Password hashing using Argon2 or BCrypt
* Email verification required

---

## Authorization

Every protected operation must verify:

```text
User
Membership
Role
Permission
```

---

## Tenant Isolation

Every query involving business data must include:

```text
TenantId
```

from authenticated context.

Cross-tenant access is prohibited.

---

# 14. Non-Functional Requirements

---

## Scalability

The architecture must support:

```text
100,000+ Users
10,000+ Organizations
Millions of Membership Records
```

---

## Extensibility

Future support for:

* Multi-organization users
* Subscription billing
* Organization ownership transfer
* SSO/SAML
* SCIM provisioning
* Audit logging
* Fine-grained permissions

must require minimal schema changes.

---

# 15. Acceptance Criteria

The implementation shall be considered complete when:

1. Users can register without creating an organization.
2. Business registration automatically creates an organization.
3. The creator receives the Owner role.
4. Users can be invited via email.
5. Memberships are stored independently from users.
6. Authorization resolves through Membership → Role → Permission.
7. JWTs contain active membership context.
8. TenantContextMiddleware resolves the tenant from membership.
9. Organization switching is supported.
10. Cross-tenant access is prevented by design.

---

## Architecture Decision Summary

**Decision:** Adopt an Organization-Centric Multi-Tenant Architecture with Membership-Based Authorization.

**Rationale:** Separating identity from tenancy provides stronger security boundaries, supports future multi-organization membership, aligns with industry-standard SaaS platforms, and eliminates the need for future database redesign when organizational complexity grows.
