using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using MuseFrontend.Models;

namespace MuseFrontend.Services;

public class ContentService
{
    private IHttpClientFactory _httpClientFactory;

    private IJSRuntime _jsRuntime;

    private ProtectedLocalStorage _localStorage;

    private AuthService _authService;

    public ContentService(
        IHttpClientFactory httpClientFactory,
        IJSRuntime jsRuntime,
        ProtectedLocalStorage localStorage,
        AuthService authService
    )
    {
        _httpClientFactory = httpClientFactory;
        _jsRuntime = jsRuntime;
        _localStorage = localStorage;
        _authService = authService;
    }

    public async Task<Post?> CreatePost(Post post)
    {
        var httpClient = _httpClientFactory.CreateClient("MuseHttpClient");

        if (!await _authService.IsAuthenticated()) return null;
        var accessToken = await _localStorage.GetAsync<string>("accessToken");

        Console.WriteLine(post);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.Value}");
        var response = await httpClient.PostAsync(
            "/content/create",
            JsonContent.Create(post, null, JsonSerializerOptions.Web)
        );

        if (!response.IsSuccessStatusCode)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "Couldn't create new post");
            return null;
        }

        var postJson = await response.Content.ReadFromJsonAsync<Post>();
        if (postJson is null)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "Error parsing ill formed response object");
        }
        await _jsRuntime.InvokeVoidAsync("alert", "Post created successfully");
        return postJson;
    }

    public async Task<List<Post>?> GetUserPosts()
    {
        var httpClient = _httpClientFactory.CreateClient("MuseHttpClient");

        if (!await _authService.IsAuthenticated()) return null;
        var accessToken = await _localStorage.GetAsync<string>("accessToken");

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.Value}");
        var response = await httpClient.GetAsync("/content/my");

        if (!response.IsSuccessStatusCode)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "Couldn't fetch user's posts");
            return null;
        }

        var postsJson = await response.Content.ReadFromJsonAsync<List<Post>>();
        if (postsJson is null)
        {
            await _jsRuntime.InvokeVoidAsync("alert, Error parsing ill formed response object");
        }
        return postsJson;
    }

    public async Task<List<Post>?> GetAllPosts()
    {
        var httpClient = _httpClientFactory.CreateClient("MuseHttpClient");

        if (!await _authService.IsAuthenticated()) return null;
        var accessToken = await _localStorage.GetAsync<string>("accessToken");

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.Value}");
        var response = await httpClient.GetAsync("/content/all");

        if (!response.IsSuccessStatusCode)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "Couldn't fetch all the posts");
            return null;
        }
        var postsJson = await response.Content.ReadFromJsonAsync<List<Post>>();
        if (postsJson is null)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "Error parsing ill formed posts object");
        }
        return postsJson;
    }

    public async Task<bool> DeletePost(string id)
    {
        var httpClient = _httpClientFactory.CreateClient("MuseHttpClient");

        if (!await _authService.IsAuthenticated()) return false;
        var accessToken = await _localStorage.GetAsync<string>("accessToken");

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.Value}");
        var response = await httpClient.DeleteAsync($"/content/{id}");

        if (!response.IsSuccessStatusCode)
        {
            await _jsRuntime.InvokeVoidAsync("alert", "Error deleting post");
            return false;
        }

        await _jsRuntime.InvokeVoidAsync("alert", "Post deleted successfully");
        return true;
    }
}