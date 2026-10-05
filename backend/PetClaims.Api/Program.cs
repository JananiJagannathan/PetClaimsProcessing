using Microsoft.EntityFrameworkCore;
using PetClaims.Api.Data;
using PetClaims.Api.Repositories;
using PetClaims.Api.Services;
using PetClaims.Api.Middleware;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// CONTROLLERS
// --------------------------------------------------

builder.Services.AddControllers();

// --------------------------------------------------
// OPENAPI / SWAGGER
// --------------------------------------------------

builder.Services.AddOpenApi();

// --------------------------------------------------
// DATABASE - EF CORE + SQLITE
// --------------------------------------------------

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

// --------------------------------------------------
// REPOSITORIES
// --------------------------------------------------

builder.Services.AddScoped<IClaimRepository, ClaimRepository>();

builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();


// --------------------------------------------------
// SERVICES
// --------------------------------------------------

builder.Services.AddScoped<IClaimService, ClaimService>();

builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
var app = builder.Build();

// --------------------------------------------------
// SWAGGER
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Pet Claims API v1");
        options.DefaultModelsExpandDepth(-1);
    });
}

// --------------------------------------------------
// GLOBAL EXCEPTION HANDLING
// --------------------------------------------------

app.UseMiddleware<GlobalExceptionMiddleware>();

// HTTPS temporarily disabled for local development
// app.UseHttpsRedirection();

app.UseAuthorization();

// --------------------------------------------------
// CONTROLLER ROUTES
// --------------------------------------------------

app.MapControllers();

// --------------------------------------------------
// SEED SAMPLE DATA
// --------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await DbSeeder.SeedAsync(context);
}

app.Run();
