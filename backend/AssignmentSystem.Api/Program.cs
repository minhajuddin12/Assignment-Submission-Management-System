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
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(feature?.Error, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

        await context.Response.WriteAsJsonAsync(new
        {
            error = "An unexpected error occurred. Please try again later."
        });
    });
});
Everything else in Program.cs — the migration/seed block, CORS, auth setup — stays exactly where it is. This one block just needs to sit early in the pipeline, before app.UseAuthentication().

What this gets you:

Any unhandled exception (a bad DB call, a null reference you didn't anticipate, etc.) now returns a clean { "error": "..." } JSON response instead of leaking a full .NET stack trace to the client — this matters for the "error handling" requirement specifically.
It's logged server-side with LogError, including the actual exception and which endpoint it hit — visible in your Render logs.
ASP.NET Core's built-in request logging (already active by default) continues logging every request's method/path/status/duration — combined with this, you already satisfy "logging" at a baseline level without touching every controller.
Optional next layer (not required, but strengthens it further): adding explicit ILogger calls inside specific controllers for meaningful business events — e.g. logging failed login attempts in AuthController, or when a teacher publishes an assignment. That's controller-by-controller work, more files touched, higher chance of another copy-paste slip like we've hit a few times tonight — so I'd only do it if you have time budget left after the README, the status-change feature, and tests.

Build and test this one first (try hitting a broken endpoint or check your Render logs after a normal request), then tell me: README next, or the teacher "change submission status" feature next?



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