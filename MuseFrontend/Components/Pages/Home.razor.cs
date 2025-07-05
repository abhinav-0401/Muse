using Microsoft.AspNetCore.Components;

using MuseFrontend.Services;

namespace MuseFrontend.Components.Pages;

public partial class Home
{
    private bool _isAuthenticated = true;

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
        if (!_isAuthenticated)
        {
            Navigation.NavigateTo("/auth/signup");
        }
    }

    Task OnMarkdownValueChanged(string value)
    {
        _markdownValue = value;

        return Task.CompletedTask;
    }
}