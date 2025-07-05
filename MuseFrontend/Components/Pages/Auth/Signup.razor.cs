using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MuseFrontend.Models;
using MuseFrontend.Services;

namespace MuseFrontend.Components.Pages.Auth;

public partial class Signup
{
    private AuthUser _user = new();

    [Inject]
    private ApiService ApiService { get; set; } = default!;

    private async Task SignupHandler(MouseEventArgs e)
    {
        await ApiService.AuthService.SignupUser(_user);
    }
}
