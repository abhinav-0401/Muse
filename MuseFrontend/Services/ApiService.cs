using MuseFrontend.Models;

namespace MuseFrontend.Services;

public class ApiService
{
    public AuthService AuthService { get; set; } = default!;
    public ContentService ContentService { get; set; } = default!;

    public ApiService(AuthService authService, ContentService contentService)
    {
        AuthService = authService;
        ContentService = contentService;
    }
}