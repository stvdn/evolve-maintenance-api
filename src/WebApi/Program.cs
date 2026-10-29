using System.Threading.RateLimiting;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.OpenApi.Models;
using WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Clean Architecture - Infrastructure layer DI
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Rate Limiting (API Gateway Pattern)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("PerUser", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MSApiMantenimientosEvolutivos",
        Version = "v1",
        Description = "API de Gestión de Mantenimientos Evolutivos y Métricas de Calidad - Banco Ecuador",
        Contact = new OpenApiContact
        {
            Name = "Célula de Mantenimientos Evolutivos",
            Email = "arquitectura@bancoecuador.com"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Middleware pipeline (API Gateway Pattern)
app.UseExceptionHandler(_ => { }); // Fallback
app.UseMiddleware<ExceptionMiddleware>(); // Custom error handling

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MSApiMantenimientosEvolutivos v1");
    options.RoutePrefix = "api-docs";
});

app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("PerUser");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "MSApiMantenimientosEvolutivos",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
