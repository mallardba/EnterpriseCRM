using EnterpriseCRM.Infrastructure.Data;
using EnterpriseCRM.Infrastructure.UnitOfWork;
using EnterpriseCRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Enterprise CRM API", Version = "v1" });
});

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Enterprise CRM API v1");
        c.RoutePrefix = "swagger";
    });
}

// Enable Swagger in all environments for testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Enterprise CRM API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application terminated unexpectedly: {ex.Message}");
}
