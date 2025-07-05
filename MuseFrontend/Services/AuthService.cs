
using System.Text.Json;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using MuseFrontend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace MuseFrontend.Services;

public class AuthService
{
    private HttpClient _http = default!;

    private ProtectedLocalStorage _localStorage = default!;

    private IJSRuntime _jsRuntime = default!;

    public AuthService(
        HttpClient http,
        ProtectedLocalStorage localStorage,
        IJSRuntime jsRuntime
    )
    {
        _http = http;
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }

    public async Task SignupUser(AuthUser authUser)
    {
        var response = await _http.PostAsync(
            "/auth/signup",
            JsonContent.Create(authUser, null, JsonSerializerOptions.Web)
        );

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            // user already exists
            await _jsRuntime.InvokeVoidAsync("alert", "User already exists");
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "Error signing up user");
            return;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (authResponse is null)
        {
            Console.Error.WriteLine("Error parsing ill formed signup response");
            return;
        }
        Console.WriteLine(authResponse.AccessToken);
        await _localStorage.SetAsync("accessToken", authResponse.AccessToken);

        var user = await GetUserInfoAsync();
        if (user is null)
        {
            Console.Error.WriteLine("Error parsing ill formed user response");
            return;
        }
        await _localStorage.SetAsync("user", user); 
    }

    public async Task LoginUser(AuthUser authUser)
    {
        var response = await _http.PostAsync(
            "/auth/login",
            JsonContent.Create(authUser, null, JsonSerializerOptions.Web)
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // user doesn't exist
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine("Error logging in user");
            return;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (authResponse is null)
        {
            Console.Error.WriteLine("Error parsing ill formed login response");
            return;
        }

        Console.WriteLine(authResponse);
        await _localStorage.SetAsync("accessToken", authResponse.AccessToken);

        var user = await GetUserInfoAsync();
        if (user is null)
        {
            Console.Error.WriteLine("Error parsing ill formed login response");
            return;
        }
        await _localStorage.SetAsync("user", user);
    }

    public async Task<User?> GetUserInfoAsync()
    {
        var token = await _localStorage.GetAsync<string>("accessToken");
        if (!token.Success)
        {
            Console.Error.WriteLine("Couldn't get the accessToken, user is not logged in");
            return null;
        }

        var response = await _http.PostAsync(
            "/auth/user",
            JsonContent.Create(new { AccessToken = token }, null, JsonSerializerOptions.Web)
        );
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine("Couldn't fetch user details");
            return null;
        }

        var user = await response.Content.ReadFromJsonAsync<User>();
        return user;
    }

    public async Task<bool> IsAuthenticated()
    {
        var tokenResult = await _localStorage.GetAsync<string>("accessToken");

        if (!tokenResult.Success) return false;

        if (!IsAccessTokenValid(tokenResult.Value))
        {
            await _localStorage.DeleteAsync("accessToken");
            return false;
        }

        var userResult = await _localStorage.GetAsync<string>("user");

        var shouldFetchUser = !userResult.Success ||
            !CheckAccessTokenPayload(tokenResult.Value, userResult.Value);

        if (!shouldFetchUser) return true;

        var response = await _http.PostAsync(
            "/auth/user",
            JsonContent.Create(new { AccessToken = tokenResult.Value }, null, JsonSerializerOptions.Web)
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // user doesn't exist
            await _localStorage.DeleteAsync("accessToken");
            return false;
        }

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<User>();
            if (user is null)
            {
                Console.Error.WriteLine("Error parsing malformed user object");
                return false;
            }
            await _localStorage.SetAsync("user", user);
        }
        return true;
    }

    private bool IsAccessTokenValid(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            Console.Error.WriteLine("Invalid JWT format.");
            return false;
        }


        var jwtToken = handler.ReadJwtToken(token);

        if (DateTime.UtcNow < jwtToken.ValidTo) return true;
        return false;
    }

    private bool CheckAccessTokenPayload(string token, string username)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(token))
        {
            Console.Error.WriteLine("Invalid JWT format.");
            return false;
        }

        var jwtToken = handler.ReadJwtToken(token);

        if (jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value == username) return true;

        return false;
    }
}