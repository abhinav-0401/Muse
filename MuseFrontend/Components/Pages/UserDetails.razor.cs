using Microsoft.AspNetCore.Components;
using MuseFrontend.Models;
using MuseFrontend.Services;

namespace MuseFrontend.Components.Pages;

public partial class UserDetails
{
    private bool _isAuthenticated = false;

    private UserInfo? _user = null;

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
        _user = await ApiService.AuthService.GetUserInfoAsync();
    }
}