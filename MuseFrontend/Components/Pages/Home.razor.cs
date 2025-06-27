using Microsoft.AspNetCore.Components;

using MuseFrontend.Services;
using MuseFrontend.Models;

namespace MuseFrontend.Components.Pages;

public partial class Home
{
    private bool _isAuthenticated = false;
    [Inject]
    private ApiService ApiService { get; set; } = default!;
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;
    string _markdownValue = "# EasyMDE \n Go ahead, play around with the editor! Be sure to check out **bold**, *italic*, [links](https://google.com) and all the other features. You can type the Markdown syntax, use the toolbar, or use shortcuts like `ctrl-b` or `cmd-b`.";

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        _isAuthenticated = await ApiService.AuthService.IsAuthenticated();
        Navigation.NavigateTo("/auth/signup");
        var user = new AuthUser
        {
            Username = "Abhinav",
            Password = "12345",
        };
        // await ApiService.AuthService.SignupUser(user);
    }

    Task OnMarkdownValueChanged(string value)
    {
        _markdownValue = value;

        return Task.CompletedTask;
    }
}