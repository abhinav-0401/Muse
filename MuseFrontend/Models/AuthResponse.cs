namespace MuseFrontend.Models;

public record AuthResponse(string Username, string AccessToken, string RefreshToken);