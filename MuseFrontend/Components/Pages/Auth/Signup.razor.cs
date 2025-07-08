using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MuseFrontend.Models;
using MuseFrontend.Services;

namespace MuseFrontend.Components.Pages.Auth;

public partial class Signup
{
    private AuthUser _user = new();

    [Inject]
    private ApiService ApiService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private async Task SignupHandler(MouseEventArgs e)
    {
        var isSuccess = await ApiService.AuthService.SignupUser(_user);
        if (!isSuccess) return;

        Navigation.NavigateTo("/");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var isAuth = await ApiService.AuthService.IsAuthenticated();
        if (isAuth) Navigation.NavigateTo("/");
    }

    private void GoToLogin(MouseEventArgs e) => Navigation.NavigateTo("/auth/login");
}
