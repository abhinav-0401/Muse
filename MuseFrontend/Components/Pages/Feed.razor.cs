using Microsoft.AspNetCore.Components;
using MuseFrontend.Models;
using MuseFrontend.Services;

namespace MuseFrontend.Components.Pages;

public partial class Feed
{
    private bool _isAuthenticated = false;

    private List<Post>? _posts = null;

    [Inject]
    private ApiService ApiService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("here");
        _isAuthenticated = await ApiService.AuthService.IsAuthenticated();
        if (!_isAuthenticated)
        {
            Navigation.NavigateTo("/auth/login");
            return;
        }
        _posts = await ApiService.ContentService.GetAllPosts();
    }
}