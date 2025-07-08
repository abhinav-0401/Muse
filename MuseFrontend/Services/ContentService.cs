using Microsoft.JSInterop;

namespace MuseFrontend.Services;

public class ContentService
{
    private HttpClient _httpClient;

    private IJSRuntime _jsRuntime;

    public ContentService(
        HttpClient httpClient,
        IJSRuntime jsRuntime
    )
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }
}