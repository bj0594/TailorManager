using Scalar.AspNetCore;
using TailorManager.Core.Repository;
using TailorManager.Core;
using Microsoft.EntityFrameworkCore;
using TailorManager.Infrastructure;
using TailorManager.Api.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddDbContext<TailorManagerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<ICustomerRepository, EfCustomerRepository>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddSingleton<IUserRepository, FakeUserRepository>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<TailorManagerDbContext>(
        customTestQuery: async (db, cancellationToken) =>
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return true;
        });

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<TailorManagerDbContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes("Bearer"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
