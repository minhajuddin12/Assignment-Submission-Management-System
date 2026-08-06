using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Services;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Teacher")]
public class TeacherController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _currentUser;

    public TeacherController(AppDbContext db, CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("subjects")]
    public async Task<IActionResult> GetMySubjects()
    {
        var subjects = await _db.TeacherSubjectAssignments
            .Where(tsa => tsa.TeacherId == _currentUser.UserId)
            .Select(tsa => new { tsa.Subject.Id, tsa.Subject.Name, tsa.Subject.ClassRoomId })
            .ToListAsync();

        return Ok(subjects);
    }
}