using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PROG7311_POE.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Register JWT Authentication for local MVC security context
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

    // Extract token from cookie for local session validation
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("jwt_token", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

// Register HttpClient and cookie JWT forwarding handler
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CookieTokenHandler>();

builder.Services.AddHttpClient("GlmsApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8081/");
})
.AddHttpMessageHandler<CookieTokenHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Redirect 401 unauthorized requests to /Account/Login for MVC routes
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 401 && 
        context.Request.Path.Value != null && 
        !context.Request.Path.Value.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Redirect("/Account/Login");
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
