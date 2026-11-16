using TailorManager.Web.Components;
using TailorManager.Web.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// The clients call the API at ApiBaseUrl. The pages only know the interfaces.
var apiBaseUrl = new Uri(builder.Configuration["ApiBaseUrl"]!);
builder.Services.AddHttpClient<ICustomersClient, CustomersClient>(client => client.BaseAddress = apiBaseUrl);
builder.Services.AddHttpClient<IOrdersClient, OrdersClient>(client => client.BaseAddress = apiBaseUrl);

// One TokenStore per browser tab. AuthClient logs in and fills it.
builder.Services.AddScoped<TokenStore>();
builder.Services.AddHttpClient<IAuthClient, AuthClient>(client => client.BaseAddress = apiBaseUrl);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
