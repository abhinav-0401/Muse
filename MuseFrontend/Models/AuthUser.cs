namespace MuseFrontend.Models;

public record AuthUser
{
    public string? Username { get; set; } = default;
    public string? Password { get; set; } = default;
}