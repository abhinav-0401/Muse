using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MuseFrontend.Models;
using MuseFrontend.Services;

namespace MuseFrontend.Components.Pages.Auth;

public partial class Login
{
    private AuthUser _user = new();

    [Inject]
    private ApiService ApiService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private async Task LoginHandler(MouseEventArgs e)
    {
        var isSuccess = await ApiService.AuthService.LoginUser(_user);
        if (!isSuccess) return;

        Navigation.NavigateTo("/");
    }

    private void GoToSignup(MouseEventArgs e) => Navigation.NavigateTo("/auth/login");

}