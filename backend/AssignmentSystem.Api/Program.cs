using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Services;
using AssignmentSystem.Api.Models;          // for User and UserRole

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<JwtService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',')
                     ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// =====================================================
// Automatic migration + seed demo users
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Apply any pending migrations
    db.Database.Migrate();

    // Seed only if Users table is empty
    if (!db.Users.Any())
    {
        // Using BCrypt (most common). 
        // If you use a different hasher in your AuthController, change the three lines below.

        var admin = new User
        {
            FullName = "System Admin",
            Email = "admin@school.com",
            Role = UserRole.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            CreatedAt = DateTime.UtcNow
        };

        var teacher = new User
        {
            FullName = "Demo Teacher",
            Email = "teacher@school.com",
            Role = UserRole.Teacher,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
            CreatedAt = DateTime.UtcNow
        };

        var student = new User
        {
            FullName = "Demo Student",
            Email = "student@school.com",
            Role = UserRole.Student,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
            CreatedAt = DateTime.UtcNow
            // ClassId can stay null for now
        };

        db.Users.AddRange(admin, teacher, student);
        db.SaveChanges();

        Console.WriteLine(">>> Demo users seeded successfully");
    }
}
// =====================================================

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();