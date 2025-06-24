namespace MuseFrontend.Models;

public record User
{
    public string? Username { get; set; } = default;
    public string? Password { get; set; } = default;
}