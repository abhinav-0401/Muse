using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MuseFrontend.Models;
using MuseFrontend.Services;

namespace MuseFrontend.Components.Pages;

public partial class AllPosts
{
    private bool _isAuthenticated = false;

    private List<Post>? _posts = null;

    [Inject]
    private ApiService ApiService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _isAuthenticated = await ApiService.AuthService.IsAuthenticated();
        if (!_isAuthenticated)
        {
            Navigation.NavigateTo("/auth/login");
            return;
        }
        _posts = await ApiService.ContentService.GetAllPosts();
    }
}

