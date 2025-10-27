using EnterpriseCRM.Infrastructure.Data;
using EnterpriseCRM.Infrastructure.UnitOfWork;
using EnterpriseCRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using EnterpriseCRM.BlazorServer.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add HttpClient for WebAPI calls
builder.Services.AddHttpClient<IProductClientService, ProductClientService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5001");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    // ⚠️ DEVELOPMENT ONLY: Allow invalid certificates
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return handler;
});

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
