using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MuseFrontend.Models;
using MuseFrontend.Services;

namespace MuseFrontend.Components.Pages;

public partial class Home
{
    private string? _mdValue = default;

    private bool _isAuthenticated = true;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private ApiService ApiService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _isAuthenticated = await ApiService.AuthService.IsAuthenticated();
        if (!_isAuthenticated)
        {
            Navigation.NavigateTo("/auth/signup");
        }
    }

    private async Task NewPostHandler()
    {
        if (string.IsNullOrEmpty(_mdValue))
        {
            await JsRuntime.InvokeVoidAsync("alert", "Nothing to post yet!");
            return;
        }
        var post = new Post(Content: _mdValue, Id: null, Userid: null, Username: null);
        post = await ApiService.ContentService.CreatePost(post);
        if (post is null) return;
        Console.WriteLine("Post is: {0}", post);
    }
}