using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs;
using AssignmentSystem.Api.Models;
using AssignmentSystem.Api.Services;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict("A user with this email already exists.");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
            return BadRequest("Role must be Admin, Teacher, or Student.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = parsedRole,
            ClassId = request.ClassId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, user.FullName, user.Email, user.Role.ToString()));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, user.FullName, user.Email, user.Role.ToString()));
    }
}