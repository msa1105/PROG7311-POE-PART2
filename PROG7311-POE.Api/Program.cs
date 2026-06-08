using Microsoft.EntityFrameworkCore;
using PROG7311_POE.Data;
using PROG7311_POE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Register Swagger services with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "GLMS Backend API",
        Version = "v1",
        Description = "Global Logistics Management System Web API"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecureDemoJwtSecretKeyGLMSProject2026!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GLMS_Issuer";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GLMS_Audience";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (connectionString == "InMemory")
    {
        options.UseInMemoryDatabase("InMemoryDbForTesting");
    }
    else
    {
        options.UseSqlServer(connectionString,
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
    }
});

builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IContractWorkflowService, ContractWorkflowService>();

var app = builder.Build();

// Enable Swagger UI middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GLMS API V1");
    c.RoutePrefix = "swagger";
});

// Seed the database with initial data and apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    int retryCount = 10;
    for (int i = 0; i < retryCount; i++)
    {
        try
        {
            if (context.Database.IsRelational())
            {
                logger.LogInformation("Applying pending migrations...");
                context.Database.Migrate();
            }
            else
            {
                logger.LogInformation("Ensuring database is created...");
                context.Database.EnsureCreated();
            }
            logger.LogInformation("Database migrated/created successfully. Seeding data...");
            DbSeeder.SeedAsync(context).Wait();
            logger.LogInformation("Database seeded successfully.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while migrating/seeding the database (attempt {i + 1} of {retryCount}).");
            if (i == retryCount - 1)
            {
                throw;
            }
            System.Threading.Thread.Sleep(5000); // Wait 5 seconds before retrying
        }
    }
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

