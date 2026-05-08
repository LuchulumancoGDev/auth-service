# Auth Service — Claude AI Instructions

## Purpose

The Auth Service is the centralized identity and access management service for the Transport SaaS platform.

This service is responsible for:

- Authentication
- Authorization
- Identity management
- Tenant creation and association
- JWT generation
- Refresh token lifecycle management
- Social authentication
- Session management
- User onboarding
- Security enforcement

This service acts as the single identity provider for the platform.

All downstream APIs trust the JWT issued by this service.

---

# High-Level Architecture

The platform uses a microservice architecture.

The Auth Service is the gateway into the ecosystem.

```text
Client Apps
    ↓
API Gateway
    ↓
Auth API
    ↓
JWT Issued
    ↓
Role-Specific APIs
 ├── Admin API
 ├── Driver API
 └── Customer API
```

The Auth Service does NOT contain transportation business logic.

It only manages identity, authentication, security, tenancy, and authorization.

---

# Core Architectural Principles

The entire solution must follow:

- SOLID principles
- Clean Architecture
- Separation of Concerns
- Explicit boundaries
- Dependency Injection
- CQRS-oriented workflows where appropriate

Avoid:

- Fat controllers
- Service locators
- Business logic inside controllers
- DbContext usage inside controllers
- Infrastructure leakage into domain
- Static helper abuse
- Multi-responsibility services

---

# Solution Structure

```text
src/
 ├── Auth.Api
 ├── Auth.Application
 ├── Auth.Domain
 ├── Auth.Infrastructure
 └── Shared
```

---

# Layer Responsibilities

## Auth.Api

Contains:

- Controllers
- Middleware
- Authentication setup
- Authorization setup
- Swagger/OpenAPI
- Dependency injection
- API filters
- Request/response handling

Must NOT contain:

- Business rules
- EF Core queries
- Repository logic
- Domain logic

Controllers must remain extremely thin.

Controllers should only:

1. Accept requests
2. Validate model state
3. Call application services
4. Return standardized responses

---

## Auth.Application

Contains:

- Application services
- Use cases
- DTOs
- CQRS handlers
- Validators
- Interfaces
- Authentication workflows
- Registration workflows
- Tenant orchestration logic

This layer coordinates business processes.

---

## Auth.Domain

Contains:

- Entities
- Enums
- Value objects
- Domain rules
- Domain events

This layer must remain framework-independent.

No EF Core attributes.

No infrastructure references.

---

## Auth.Infrastructure

Contains:

- EF Core
- DbContext
- Repository implementations
- JWT token generation
- OAuth integrations
- OpenID Connect integrations
- Social authentication providers
- External services
- Persistence
- Cryptography
- Email providers
- Refresh token storage

---

# Multi-Tenancy Architecture

## Tenancy Strategy

The system uses:

- Shared database
- Shared schema
- Tenant isolation

Every tenant-scoped entity MUST inherit:

```csharp
public abstract class TenantEntity : Entity
{
    public Guid TenantId { get; set; }
}
```

Base entity:

```csharp
public abstract class Entity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

# Tenant Entity

```csharp
public class Tenant : Entity
{
    public string Name { get; set; }
    public TenantType Type { get; set; }
    public bool IsActive { get; set; }

    public ICollection<User> Users { get; set; }
}
```

---

# Tenant Types

```csharp
public enum TenantType
{
    Individual,
    Organization
}
```

---

# Tenant Isolation Rules

Tenant isolation is NON-NEGOTIABLE.

All tenant filtering MUST happen automatically using:

- EF Core Global Query Filters

Never manually apply TenantId filtering repeatedly in services.

Tenant context must come from:

- JWT claims
- Current authenticated user context

Never trust TenantId from client requests.

---

# Identity Model

## User Entity Requirements

User entity must support:

- Local authentication
- OAuth authentication
- OpenID Connect authentication
- Multiple external providers
- Refresh tokens
- Audit information

Example structure:

```csharp
public class User : TenantEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string EmailAddress { get; set; }

    public string PasswordHash { get; set; }

    public Guid RoleId { get; set; }
    public Role Role { get; set; }

    public bool IsEmailVerified { get; set; }

    public bool IsActive { get; set; }

    public string? ExternalProvider { get; set; }
    public string? ExternalProviderId { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; }
}
```

---

# Role Architecture

## Fixed Roles ONLY

The system ONLY supports:

- Admin
- Driver
- Customer

Roles are:

- Seeded
- Immutable
- Internal
- System-defined

Users MUST NOT:

- Create roles
- Modify roles
- Delete roles
- Create permissions
- Modify permissions

---

# One User = One Role

Each user has exactly ONE role.

Do NOT implement:

- UserRoles tables
- Multi-role systems
- Dynamic role assignment frameworks

This is intentional for:

- Simpler JWTs
- Predictable authorization
- API separation
- Reduced complexity
- Security consistency

---

# Permission Model

Permissions are internal system permissions only.

Permissions are NOT user-managed.

Permissions exist for:

- Internal authorization policies
- Fine-grained backend checks
- Future expansion

---

# Authentication Architecture

The system supports:

## Local Authentication

Using:

- Email/password
- BCrypt password hashing

Requirements:

- Secure password hashing
- Password complexity validation
- Email uniqueness
- Account lockout policies
- Failed login tracking

---

# Social Authentication

The Auth Service must support:

| Provider  | Protocol                |
| --------- | ----------------------- |
| Google    | OAuth2 + OpenID Connect |
| Microsoft | OAuth2 + OpenID Connect |
| Facebook  | OAuth2                  |
| Twitter/X | OAuth2                  |

---

# Social Authentication Requirements

The implementation must support:

- Account creation
- Account linking
- Login
- JWT generation
- Refresh token generation

The system must normalize all providers into ONE unified User model.

---

# OpenID Connect Requirements

Google and Microsoft integrations must use:

- OpenID Connect
- OAuth2 Authorization Code Flow

Retrieve:

- Email
- First name
- Last name
- Profile picture (optional)
- Provider unique identifier

Validate:

- ID tokens
- Token signatures
- Issuer
- Audience
- Expiration

---

# OAuth2 Requirements

Facebook and Twitter/X integrations must support:

- Authorization Code Flow
- Secure callback handling
- State validation
- CSRF protection

---

# External Login Flow

## First-Time Social Login

Flow:

1. User authenticates with provider
2. Provider returns identity
3. System checks for existing user
4. If not found:
   - Create tenant
   - Create user
   - Assign role
5. Generate JWT
6. Generate refresh token
7. Return auth response

---

# Existing Social User Login

Flow:

1. Validate provider token
2. Match provider ID
3. Generate JWT
4. Rotate refresh token
5. Return authentication response

---

# Account Linking Rules

A user may authenticate using multiple providers.

Examples:

- Local + Google
- Local + Microsoft
- Google + Facebook

Provider identities must map to the same user safely.

---

# JWT Requirements

JWTs must include:

```json
{
  "sub": "userId",
  "tenantId": "tenantId",
  "role": "Admin",
  "email": "user@email.com"
}
```

JWTs are stateless.

JWTs are the ONLY authorization mechanism between services.

---

# Refresh Token Architecture

Refresh tokens MUST support:

- Rotation
- Revocation
- Expiration
- Device/session tracking
- Reuse detection

Refresh tokens must:

- Be hashed before storage
- Never be stored plaintext
- Support multiple active sessions

---

# Registration Flows

## Individual Registration

Flow:

1. User selects Individual
2. New tenant created
3. User assigned selected role
4. User linked to tenant
5. JWT issued
6. Refresh token generated

Supported roles:

- Driver
- Customer

---

# Organization Registration

Flow:

1. User selects Organization
2. Business name provided
3. Organization tenant created
4. User assigned Admin role
5. JWT issued
6. Refresh token generated

---

# Organization User Management

Organization Admins can:

- Invite users
- Upload users via Excel
- Assign predefined roles

Only Admins can manage organization users.

---

# Authorization Architecture

Authorization is role-based.

## API Separation

| API          | Allowed Role |
| ------------ | ------------ |
| Admin API    | Admin        |
| Driver API   | Driver       |
| Customer API | Customer     |

JWT role claim determines API access.

---

# Security Requirements

Mandatory requirements:

- BCrypt password hashing
- Refresh token rotation
- HTTPS enforcement
- Secure cookie support where applicable
- CSRF protection for OAuth
- OAuth state validation
- Tenant isolation
- Role validation
- Secure JWT signing
- Secure secret management

Never:

- Store plaintext passwords
- Trust client TenantIds
- Allow runtime role modification
- Expose permissions publicly

---

# Middleware Requirements

The API layer must implement:

- Global exception handling middleware
- Authentication middleware
- Authorization middleware
- Correlation ID middleware
- Structured logging middleware
- Request tracing

---

# Database Architecture

## Database

Auth Service uses:

- SQL Server
- EF Core

---

# Migrations

Migrations belong ONLY in:

```text
Auth.Infrastructure
```

Never place migrations inside API projects.

---

# Repository Pattern

Use generic repository pattern.

Repositories belong ONLY in Infrastructure.

Required repositories include:

- UserRepository
- TenantRepository
- RoleRepository
- PermissionRepository
- RefreshTokenRepository

---

# Event-Driven Architecture

The Auth Service publishes domain/integration events.

Examples:

- UserRegistered
- UserLoggedIn
- UserCreatedFromSocialProvider
- RefreshTokenRevoked
- PasswordResetRequested
- OrganizationCreated

Events are consumed asynchronously by workers and other services.

---

# Validation Requirements

Use:

- FluentValidation

Validation belongs in:

- Application layer

Never inside controllers.

---

# Logging & Observability

Implement:

- Structured logging
- Correlation IDs
- Audit logs
- Failed login monitoring
- Security event tracking

Sensitive data must never be logged.

Never log:

- Passwords
- Tokens
- Secrets

---

# Performance Requirements

Avoid:

- N+1 queries
- Large object graphs
- Excessive includes

Use:

- Async operations
- Query projections
- Pagination
- AsNoTracking where appropriate

---

# Coding Standards

## Naming

GOOD:

- CurrentTenantProvider
- JwtTokenGenerator
- RefreshTokenService
- RegisterUserCommand
- SocialAuthenticationService

BAD:

- Helper
- Utils
- Manager
- CommonService

---

# Dependency Injection

Use constructor injection ONLY.

Avoid:

- Static services
- Service locators

---

# Async Rules

All I/O operations must be async.

---

# API Response Standards

All endpoints should return standardized API responses.

Use consistent:

- Success responses
- Error responses
- Validation responses

---

# Claude Behavioral Instructions

When generating code:

- Follow Clean Architecture strictly
- Keep controllers thin
- Keep domain pure
- Respect tenant isolation
- Respect one-user-one-role architecture
- Never introduce multi-role complexity
- Use explicit naming
- Use async patterns
- Use secure authentication defaults

When uncertain:

- Ask before changing architecture
- Do NOT over-engineer
- Do NOT introduce unnecessary abstractions
- Do NOT introduce frameworks not already required

---

# Non-Negotiable Constraints

- One user = one role
- Roles are immutable
- Permissions are internal
- Tenant isolation is mandatory
- JWT drives authorization
- Auth Service is the centralized identity provider
- APIs are role-separated
- TenantId never comes from client input
- Global query filters enforce tenant isolation
- Social providers must use OAuth2/OpenID Connect correctly
- Refresh tokens must rotate securely

---

# Expected Outcome

The Auth Service must be:

- Secure
- Scalable
- Multi-tenant aware
- Cleanly architected
- Event-driven
- Extensible
- Production-ready
- OAuth2/OpenID compliant
- Easy to maintain
- Easy to evolve
