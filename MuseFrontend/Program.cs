using System.Net;
using MuseFrontend.Components;
using MuseFrontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("MuseHttpClient", httpClient =>
{
#if DEBUG
    httpClient.BaseAddress = new Uri("http://localhost:4321/");
#else
    httpClient.BaseAddress = new Uri("https://muse-0f55.onrender.com");
#endif
});
builder.Services
    .AddScoped<ApiService>()
    .AddScoped<AuthService>()
    .AddScoped<ContentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
