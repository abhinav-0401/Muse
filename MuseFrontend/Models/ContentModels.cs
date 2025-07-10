namespace MuseFrontend.Models;

public record User(string Username);

public record Post(string Content, string ContentHtml, string? Id, string? Userid, string? Username);