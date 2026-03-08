using System.Data;
using AmlScreening.Infrastructure;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// In Development, listen on http://localhost:5001 so the Angular app (environment.apiUrl) can reach the API
if (builder.Environment.IsDevelopment())
    builder.WebHost.UseUrls("http://localhost:5001");

builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AmlScreening API v1"));
}

// Only redirect HTTP to HTTPS when not in Development so http://localhost:5001 works from the UI
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await db.Database.MigrateAsync();
    }
    catch (SqlException ex) when (ex.Number == 2714 || ex.Number == 1913) // Object already exists / column already exists
    {
        throw new InvalidOperationException(
            "The database already contains tables from a previous migration. Drop the database and run again. " +
            "From the aml folder run: dotnet ef database drop --project src/AmlScreening.Infrastructure --startup-project src/AmlScreening.Api --force",
            ex);
    }
}

app.Run();
