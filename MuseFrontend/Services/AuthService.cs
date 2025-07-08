
using System.Text.Json;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using MuseFrontend.Models;
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

    public async Task<bool> SignupUser(AuthUser authUser)
    {
        var response = await _http.PostAsync(
            "/auth/signup",
            JsonContent.Create(authUser, null, JsonSerializerOptions.Web)
        );

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            // user already exists
            await _jsRuntime.InvokeVoidAsync("alert", "User already exists");
            return false;
        }

        if (!response.IsSuccessStatusCode)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "Error signing up user");
            return false;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (authResponse is null)
        {
            Console.Error.WriteLine("Error parsing ill formed signup response");
            return false;
        }
        Console.WriteLine(authResponse.AccessToken);
        await _localStorage.SetAsync("accessToken", authResponse.AccessToken);
        await _localStorage.SetAsync("refreshToken", authResponse.RefreshToken);

        var user = await GetUserInfoAsync();
        if (user is null)
        {
            Console.Error.WriteLine("Error parsing ill formed user response");
            return true;
        }
        await _localStorage.SetAsync("user", user);
        await _jsRuntime.InvokeVoidAsync("alert", $"{user.Username}'s account has been successfully created");
        return true;
    }

    public async Task<bool> LoginUser(AuthUser authUser)
    {
        var response = await _http.PostAsync(
            "/auth/login",
            JsonContent.Create(authUser, null, JsonSerializerOptions.Web)
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // user doesn't exist
            await _jsRuntime.InvokeVoidAsync("alert", "User not found");
            return false;
        }

        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine("Error logging in user");
            await _jsRuntime.InvokeVoidAsync("alert", "Error logging in user");
            return false;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (authResponse is null)
        {
            Console.Error.WriteLine("Error parsing ill formed login response");
            return false;
        }

        Console.WriteLine("loginResponse: {0}", authResponse);
        await _localStorage.SetAsync("accessToken", authResponse.AccessToken);
        await _localStorage.SetAsync("refreshToken", authResponse.RefreshToken);

        var user = await GetUserInfoAsync();
        if (user is null)
        {
            Console.Error.WriteLine("Error parsing ill formed login response");
            return false;
        }
        await _localStorage.SetAsync("user", user);
        await _jsRuntime.InvokeVoidAsync("alert", $"{user.Username}'s account has been logged in");
        return true;
    }

    public async Task<User?> GetUserInfoAsync()
    {
        var token = await _localStorage.GetAsync<string>("accessToken");
        if (!token.Success)
        {
            Console.Error.WriteLine("Couldn't get the accessToken, user is not logged in");
            return null;
        }

        _http.DefaultRequestHeaders.Add("Authentication", $"Bearer {token.Value}");
        var response = await _http.GetAsync("/auth/user");
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine("Couldn't fetch user details");
            return null;
        }

        var strUser = await response.Content.ReadAsStringAsync();
        Console.WriteLine("string user GetUserInfoAsync(): {0}", strUser);
        var user = await response.Content.ReadFromJsonAsync<User>();
        return user;
    }

    public async Task<bool> IsAuthenticated()
    {
        var tokenResult = await _localStorage.GetAsync<string>("accessToken");
        if (!tokenResult.Success) return false;

        if (IsAccessTokenValid(tokenResult.Value!))
        {
            Console.WriteLine("valid");
            return true;
        }

        Console.WriteLine("invalid");
        await _localStorage.DeleteAsync("accessToken");

        var refreshToken = await _localStorage.GetAsync<string>("refreshToken");
        if (!refreshToken.Success) return false;

        Console.WriteLine("refreshToken: {0}", refreshToken.Value);
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {refreshToken.Value}");

        var response = await _http.GetAsync("/auth/access-token");
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "Couldn't generate new access token");
            return false;
        }

        var tokenRes = await response.Content.ReadAsStringAsync();
        Console.WriteLine("IsAuthenticated: {0}", tokenRes); 
        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        if (token is null)
        {
            Console.WriteLine("Error parsing ill formed access token json");
            return false;
        }

        await _localStorage.SetAsync("accessToken", token.AccessToken);
        Console.WriteLine("new accessToken", token.AccessToken);
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

    private bool CheckAccessTokenPayload(string token, User user)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(token))
        {
            Console.Error.WriteLine("Invalid JWT format.");
            return false;
        }

        var jwtToken = handler.ReadJwtToken(token);

        if (jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value == user.Username) return true;

        return false;
    }
}