namespace AuthApi.Controllers;

public class RefreshRequest
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class RevokeRequest
{
    public string RefreshToken { get; set; }
}