using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs;
using AssignmentSystem.Api.Models;
using AssignmentSystem.Api.Services;
public record UpdateUserRequest(string FullName, string Email, string Role, int? ClassId);

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
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound("User not found.");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
            return BadRequest("Role must be Admin, Teacher, or Student.");

        var emailTaken = await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != id);
        if (emailTaken) return Conflict("A user with this email already exists.");

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Role = parsedRole;
        user.ClassId = parsedRole == UserRole.Student ? request.ClassId : null;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound("User not found.");

        if (user.Role == UserRole.Teacher)
        {
            var hasAssignments = await _db.Assignments.AnyAsync(a => a.CreatedByTeacherId == id);
            var hasSubjectLinks = await _db.TeacherSubjectAssignments.AnyAsync(t => t.TeacherId == id);
            if (hasAssignments || hasSubjectLinks)
                return Conflict("This teacher has assignments or subject assignments linked to them. Reassign or remove those first.");
        }

        if (user.Role == UserRole.Student)
        {
            var hasSubmissions = await _db.Submissions.AnyAsync(s => s.StudentId == id);
            if (hasSubmissions)
                return Conflict("This student has submissions on record. Deleting would erase their grades and work — remove submissions first if you're sure.");
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return Ok();
    }
}