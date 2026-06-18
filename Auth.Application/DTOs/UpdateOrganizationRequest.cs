namespace Auth.Application.DTOs;

public class UpdateOrganizationRequest
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
}