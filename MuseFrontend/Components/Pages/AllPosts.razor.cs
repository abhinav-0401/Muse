using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
        Console.WriteLine("here");
        _isAuthenticated = await ApiService.AuthService.IsAuthenticated();
        if (!_isAuthenticated)
        {
            Navigation.NavigateTo("/auth/login");
            return;
        }
        _posts = await ApiService.ContentService.GetAllPosts();
    }

    private async Task DeletePostHandler(Post post)
    {
        Console.WriteLine("post in postcard: {0}", post.Id);
        await ApiService.ContentService.DeletePost(post.Id!);
        _posts = await ApiService.ContentService.GetAllPosts();
    }
}

