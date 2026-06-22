using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PRM.Application.Interfaces;
using PRM.Application.Services;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;
using PRM.Infrastructure.Repositories;
using PRM.Infrastructure.Services;
using PRM.API.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger for JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PRM API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Database
builder.Services.AddDbContext<PrmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IAllocationRepository, AllocationRepository>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();

builder.Services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Register HttpClient for LLM Providers with extended timeout for slow models
builder.Services.AddHttpClient("LlmClient", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddHttpClient();

// LLM Provider orchestration
builder.Services.AddScoped<ILlmProvider, DynamicLlmProvider>();
builder.Services.AddScoped<IAiService, AiService>();

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Background Scheduler
builder.Services.AddHostedService<BackgroundSchedulerService>();

// Auth Config
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    // Admin Policies
    options.AddPolicy("Users.Manage", policy => policy.RequireClaim("Permission", "Users.Manage"));
    options.AddPolicy("Resources.Manage", policy => policy.RequireClaim("Permission", "Resources.Manage"));
    options.AddPolicy("Projects.Manage", policy => policy.RequireClaim("Permission", "Projects.Manage"));
    options.AddPolicy("System.Configure", policy => policy.RequireClaim("Permission", "System.Configure"));
    options.AddPolicy("Allocations.ViewAll", policy => policy.RequireClaim("Permission", "Allocations.ViewAll"));

    // Manager Policies
    options.AddPolicy("Resources.Allocate", policy => policy.RequireClaim("Permission", "Resources.Allocate"));
    options.AddPolicy("Projects.ViewTeam", policy => policy.RequireClaim("Permission", "Projects.ViewTeam"));
    options.AddPolicy("Timesheets.ViewTeam", policy => policy.RequireClaim("Permission", "Timesheets.ViewTeam"));

    // Resource Policies
    options.AddPolicy("Timesheets.SubmitOwn", policy => policy.RequireClaim("Permission", "Timesheets.SubmitOwn"));
    options.AddPolicy("Allocations.ViewOwn", policy => policy.RequireClaim("Permission", "Allocations.ViewOwn"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Initialize DB and Seed Admin
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PrmDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DbInitializer.InitializeAsync(context, config);
}

app.Run();
