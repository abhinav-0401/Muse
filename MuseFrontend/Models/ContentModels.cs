namespace MuseFrontend.Models;

public record User(string Username);

public record Post(string Content, string? Id, string? Userid, string? Username);