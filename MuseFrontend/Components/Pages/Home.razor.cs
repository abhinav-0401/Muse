using Microsoft.AspNetCore.Components;

using MuseFrontend.Services;
using MuseFrontend.Models;

namespace MuseFrontend.Components.Pages;

public partial class Home
{
    [Inject]
    private ApiService ApiService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var user = new User
        {
            Username = "Abhinav",
            Password = "12345",
        };
        // await ApiService.AuthService.SignupUser(user);
    }
}