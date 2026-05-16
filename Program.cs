using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using KycApi.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ----- JWT KEY -----
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your_super_secret_key_here_123";
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

// ----- Services -----
builder.Services.AddControllers();
builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KycApi",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer <token>'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{ }
        }
    });
});

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization();

// ----- Database -----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=localhost;Database=KycDb;Trusted_Connection=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// ----- Middleware -----
// Enable Swagger in all environments (not just Development)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KycApi v1");
    c.RoutePrefix = string.Empty; // Swagger at root
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Optional: test connection string in browser
app.MapGet("/test-connection-string", () => connectionString);

app.Run();
