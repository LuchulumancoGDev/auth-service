using Auth.Application.DTOs;
using Auth.Application.Services;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ITokenHashingService _tokenHashingService;
    private readonly ISocialAuthenticationService _socialAuthService;

    public AuthenticationService(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IRoleRepository roleRepository,
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        ITokenHashingService tokenHashingService,
        ISocialAuthenticationService socialAuthService)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _roleRepository = roleRepository;
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _tokenHashingService = tokenHashingService;
        _socialAuthService = socialAuthService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (request.AccountType == AccountType.Organization && request.UserType != UserType.Admin)
            return AuthResponse.CreateError("Organization accounts must have Admin role");

        if (request.AccountType == AccountType.Individual && request.UserType == UserType.Admin)
            return AuthResponse.CreateError("Individual accounts cannot have Admin role");

        if (await _userRepository.EmailExistsAsync(request.Email))
            return AuthResponse.CreateError("Email already registered");

        var role = await _roleRepository.GetByNameAsync(request.UserType.ToString());
        if (role == null)
            return AuthResponse.CreateError("Invalid user type");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.TenantName ?? $"{request.FullName}'s Account",
            Type = request.AccountType == AccountType.Organization ? TenantType.Organization : TenantType.Individual,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _tenantRepository.AddAsync(tenant);
        await _tenantRepository.SaveChangesAsync();

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            TenantId = tenant.Id,
            AccountType = request.AccountType,
            UserType = request.UserType,
            RoleId = role.Id,
            IsEmailVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return AuthResponse.CreateError(string.Join(", ", result.Errors.Select(e => e.Description)));

        var (accessToken, refreshToken) = await GenerateTokensAsync(user);

        return AuthResponse.CreateSuccess("Registration successful", MapToUserDto(user), new JwtTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 60
        });
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return AuthResponse.CreateError("Invalid email or password");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return AuthResponse.CreateError("Invalid email or password");

        if (!user.IsActive)
            return AuthResponse.CreateError("Account is deactivated");

        var (accessToken, refreshToken) = await GenerateTokensAsync(user);

        return AuthResponse.CreateSuccess("Login successful", MapToUserDto(user), new JwtTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 60
        });
    }

    public async Task<AuthResponse> SocialLoginAsync(SocialLoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            SocialUserInfo socialUserInfo;
            switch (request.Provider.ToLower())
            {
                case "google":
                    socialUserInfo = await _socialAuthService.ValidateGoogleTokenAsync(request.AccessToken, request.IdToken, cancellationToken);
                    break;
                case "microsoft":
                    socialUserInfo = await _socialAuthService.ValidateMicrosoftTokenAsync(request.AccessToken, request.IdToken, cancellationToken);
                    break;
                case "facebook":
                    socialUserInfo = await _socialAuthService.ValidateFacebookTokenAsync(request.AccessToken, cancellationToken);
                    break;
                case "twitter":
                    socialUserInfo = await _socialAuthService.ValidateTwitterTokenAsync(request.AccessToken, cancellationToken);
                    break;
                default:
                    return AuthResponse.CreateError($"Unsupported social provider: {request.Provider}");
            }

            // Check if user exists by external provider
            var users = await _userRepository.GetAllAsync();
            var existingUser = users.FirstOrDefault(u => 
                u.ExternalProvider == socialUserInfo.Provider && 
                u.ExternalProviderId == socialUserInfo.ProviderId);

            if (existingUser != null)
            {
                if (!existingUser.IsActive)
                    return AuthResponse.CreateError("Account is deactivated");

                var (accessToken, refreshToken) = await GenerateTokensAsync(existingUser);

                return AuthResponse.CreateSuccess("Login successful", MapToUserDto(existingUser), new JwtTokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = 60
                });
            }

            // Check if email is already registered
            var existingByEmail = await _userRepository.GetByEmailAsync(socialUserInfo.Email);
            if (existingByEmail != null)
            {
                existingByEmail.ExternalProvider = socialUserInfo.Provider;
                existingByEmail.ExternalProviderId = socialUserInfo.ProviderId;

                var updateResult = await _userManager.UpdateAsync(existingByEmail);
                if (!updateResult.Succeeded)
                    return AuthResponse.CreateError("Failed to link social account");

                var (accessToken, refreshToken) = await GenerateTokensAsync(existingByEmail);

                return AuthResponse.CreateSuccess("Account linked and login successful", MapToUserDto(existingByEmail), new JwtTokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = 60
                });
            }

            // New user - create account
            var role = await _roleRepository.GetByNameAsync(UserType.Customer.ToString());
            if (role == null)
                return AuthResponse.CreateError("Invalid user type");

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = $"{socialUserInfo.FirstName}'s Account",
                Type = TenantType.Individual,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _tenantRepository.AddAsync(tenant);
            await _tenantRepository.SaveChangesAsync();

            var newUser = new ApplicationUser
            {
                UserName = socialUserInfo.Email,
                Email = socialUserInfo.Email,
                FullName = $"{socialUserInfo.FirstName} {socialUserInfo.LastName}".Trim(),
                TenantId = tenant.Id,
                AccountType = AccountType.Individual,
                UserType = UserType.Customer,
                RoleId = role.Id,
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ExternalProvider = socialUserInfo.Provider,
                ExternalProviderId = socialUserInfo.ProviderId
            };

            var createResult = await _userManager.CreateAsync(newUser);
            if (!createResult.Succeeded)
                return AuthResponse.CreateError(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            var (newAccessToken, newRefreshToken) = await GenerateTokensAsync(newUser);

            return AuthResponse.CreateSuccess("Registration successful", MapToUserDto(newUser), new JwtTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 60
            });
        }
        catch (InvalidOperationException ex)
        {
            return AuthResponse.CreateError(ex.Message);
        }
        catch (Exception)
        {
            return AuthResponse.CreateError("An error occurred during social login");
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = _tokenHashingService.HashToken(refreshToken);

        var allTokens = await _refreshTokenRepository.GetAllAsync();
        var storedToken = allTokens.FirstOrDefault(rt => rt.TokenHash == refreshTokenHash && !rt.IsRevoked);

        if (storedToken == null)
            return AuthResponse.CreateError("Invalid refresh token");

        if (storedToken.ExpiryDate <= DateTime.UtcNow)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.SaveChangesAsync();
            return AuthResponse.CreateError("Refresh token expired");
        }

        var user = await _userRepository.GetByIdWithTenantAsync(storedToken.UserId);
        if (user == null || !user.IsActive)
            return AuthResponse.CreateError("User not found or inactive");

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.SaveChangesAsync();

        var (newAccessToken, newRefreshToken) = await GenerateTokensAsync(user);

        return AuthResponse.CreateSuccess("Token refreshed successfully", MapToUserDto(user), new JwtTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 60
        });
    }

    private async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(ApplicationUser user)
    {
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = _tokenHashingService.HashToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = refreshTokenHash,
            UserId = user.Id,
            TenantId = user.TenantId,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _refreshTokenRepository.SaveChangesAsync();

        return (accessToken, refreshToken);
    }

    private static UserDto MapToUserDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        FullName = user.FullName,
        TenantId = user.TenantId,
        UserType = user.UserType.ToString(),
        IsEmailVerified = user.IsEmailVerified
    };
}