namespace MuseFrontend.Models;

public record UserInfo(string Id, string Username);

public record Post(string Content, string ContentHtml, string? Id, string? Userid, string? Username);