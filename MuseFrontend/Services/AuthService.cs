
using System.Text.Json;
using System.Net;
using Microsoft.JSInterop;
using MuseFrontend.Models;
using Microsoft.AspNetCore.Components;

namespace MuseFrontend.Services;

public class AuthService
{
    private HttpClient _http = default!;
    private IJSRuntime _jsRuntime = default!;
    private NavigationManager _navigation = default!;

    public AuthService(HttpClient http, IJSRuntime jsRuntime, NavigationManager navigation)
    {
        _http = http;
        _jsRuntime = jsRuntime;
        _navigation = navigation;
    }

    public async Task SignupUser(User user)
    {
        var response = await _http.PostAsync("/auth/signup", JsonContent.Create(user, null, JsonSerializerOptions.Web));

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            // user already exists
            return;
        }

        if (response.IsSuccessStatusCode)
        {
            var authRes = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authRes is null)
            {
                Console.Error.WriteLine("Error parsing ill formed signup response");
                return;
            }

            Console.WriteLine(authRes);

            await _jsRuntime.InvokeVoidAsync("Muse.WriteCookie", "accessToken", authRes.AccessToken, 60);
            var accessTokenCookie = await _jsRuntime.InvokeAsync<string>("Muse.ReadCookie", "accessToken");
            Console.WriteLine("Cookie is: {0}", accessTokenCookie);
        }
    }

    public async Task LoginUser(User user)
    {
        var response = await _http.PostAsync("/auth/login", JsonContent.Create(user, null, JsonSerializerOptions.Web));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // user doesn't exist
            return;
        }

        if (response.IsSuccessStatusCode)
        {
            var authRes = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authRes is null)
            {
                Console.Error.WriteLine("Error parsing ill formed login response");
                return;
            }

            Console.WriteLine(authRes);

            await _jsRuntime.InvokeVoidAsync("Muse.WriteCookie", "accessToken", authRes.AccessToken, 60);
            var accessTokenCookie = await _jsRuntime.InvokeAsync<string>("Muse.ReadCookie", "accessToken");
            Console.WriteLine("Cookie is: {0}", accessTokenCookie);
        }
    }

    public async Task VerifyAuth()
    {
        var accessToken = await _jsRuntime.InvokeAsync<string?>("Muse.ReadCookie", "accessToken");
        if (string.IsNullOrEmpty(accessToken))
        {
            _navigation.NavigateTo("/auth/login");
            return;
        }
    }
}