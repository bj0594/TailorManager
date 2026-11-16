using TailorManager.Web.Components;
using TailorManager.Web.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// The pages get CustomersClient when they ask for ICustomersClient. ApiBaseUrl is the address of the API.
builder.Services.AddHttpClient<ICustomersClient, CustomersClient > (client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!));
builder.Services.AddHttpClient<IOrdersClient, OrdersClient > (client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!));

// One TokenStore per browser tab. AuthClient logs in and fills it.
builder.Services.AddScoped<TokenStore>();
builder.Services.AddHttpClient<IAuthClient, AuthClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
