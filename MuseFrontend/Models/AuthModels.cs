namespace MuseFrontend.Models;

public record AuthResponse(string Id, string Username, string AccessToken, string RefreshToken);

public record AuthUser
{
    public string? Username { get; set; } = default;
    public string? Password { get; set; } = default;
}