using Microsoft.AspNetCore.Components;

using MuseFrontend.Services;

namespace MuseFrontend.Components.Pages;

public partial class Home
{
    private string? _mdValue = default;

    private bool _isAuthenticated = true;

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

    }
}