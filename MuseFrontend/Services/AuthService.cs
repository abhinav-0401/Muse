
using System.Text.Json;
using System.Net;
using Microsoft.JSInterop;
using MuseFrontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MuseFrontend.Services;

public class AuthService
{
    private HttpClient _http = default!;
    private IJSRuntime _jsRuntime = default!;
    private NavigationManager _navigation = default!;
    private ProtectedLocalStorage _localStorage = default!;

    public AuthService(HttpClient http, IJSRuntime jsRuntime, NavigationManager navigation)
    {
        _http = http;
        _jsRuntime = jsRuntime;
        _navigation = navigation;
    }

    public async Task SignupUser(AuthUser user)
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

            await _localStorage.SetAsync("accessToken", authRes.AccessToken);
            var accessToken = await _localStorage.GetAsync<string>("accessToken");
            Console.WriteLine("Cookie is: {0}", accessToken);
        }
    }

    public async Task LoginUser(AuthUser user)
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

            await _localStorage.SetAsync("accessToken", authRes.AccessToken);
            var result = await _localStorage.GetAsync<string>("accessToken");
            if (result.Success)
                Console.WriteLine("Cookie is: {0}", result.Value);
            else
                Console.Error.WriteLine("No accessToken in LocalStorage");
        }
    }

    public async Task VerifyAuth()
    {
        var tokenResult = await _localStorage.GetAsync<string>("accessToken");

        if (!tokenResult.Success)
        {
            _navigation.NavigateTo("/auth/login");
            return;
        }

        var userResult = await _localStorage.GetAsync<string>("user");

        var shouldFetchUser = !userResult.Success ||
            !CheckAccessTokenPayload(tokenResult.Value, userResult.Value);

        if (!shouldFetchUser)
            return;

        var response = await _http.PostAsync(
            "/auth/user",
            JsonContent.Create(new { AccessToken = tokenResult.Value }, null, JsonSerializerOptions.Web)
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // user doesn't exist
            await _localStorage.DeleteAsync("accessToken");
            return;
        }

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<User>();
            if (user is null)
            {
                Console.Error.WriteLine("Error parsing malformed user object");
                return;
            }
            await _localStorage.SetAsync("user", user);
        }
    }

    private bool CheckAccessTokenPayload(string accessToken, string username)
    {
        if (accessToken != username)
        {
            return false;
        }
        return true;
    }
}