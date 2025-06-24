using MuseFrontend.Models;

namespace MuseFrontend.Services;

public class ApiService
{
    private HttpClient _http = default!;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task LoginUser(User user)
    {
        var response = await _http.PostAsync("/auth/login", JsonContent.Create(user));
        response.EnsureSuccessStatusCode();
        var strResult = await response.Content.ReadAsStringAsync();
        Console.WriteLine(strResult);
    }
}