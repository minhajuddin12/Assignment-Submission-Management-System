using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models;
public record UpdateUserRequest(string FullName, string Email, string Role, int? ClassId);
namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    public record CreateClassRoomRequest(string Name);
    public record CreateSubjectRequest(string Name, int ClassRoomId);
    public record AssignTeacherRequest(int TeacherId, int SubjectId);
    public record AssignTeacherToClassRequest(int TeacherId, int ClassRoomId, int SubjectId);
    public record UpdateStudentClassRequest(int StudentId, int ClassRoomId);

    [HttpPost("classrooms")]
    public async Task<IActionResult> CreateClassRoom(CreateClassRoomRequest request)
    {
        var classRoom = new ClassRoom { Name = request.Name };
        _db.ClassRooms.Add(classRoom);
        await _db.SaveChangesAsync();
        return Ok(new { classRoom.Id, classRoom.Name });
    }

    [HttpGet("classrooms")]
    public async Task<IActionResult> GetClassRooms()
    {
        var classRooms = await _db.ClassRooms.Select(c => new { c.Id, c.Name }).ToListAsync();
        return Ok(classRooms);
    }

    [HttpPost("subjects")]
    public async Task<IActionResult> CreateSubject(CreateSubjectRequest request)
    {
        var classRoom = await _db.ClassRooms.FindAsync(request.ClassRoomId);
        if (classRoom is null) return NotFound("ClassRoom not found.");

        var subject = new Subject { Name = request.Name, ClassRoomId = request.ClassRoomId };
        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync();
        return Ok(new { subject.Id, subject.Name, subject.ClassRoomId });
    }

    [HttpGet("subjects")]
    public async Task<IActionResult> GetSubjects()
    {
        var subjects = await _db.Subjects.Select(s => new { s.Id, s.Name, s.ClassRoomId }).ToListAsync();
        return Ok(subjects);
    }

    [HttpPost("assign-teacher")]
    public async Task<IActionResult> AssignTeacher(AssignTeacherRequest request)
    {
        var teacher = await _db.Users.FindAsync(request.TeacherId);
        if (teacher is null || teacher.Role != UserRole.Teacher) return BadRequest("Invalid teacher.");

        var subject = await _db.Subjects.FindAsync(request.SubjectId);
        if (subject is null) return NotFound("Subject not found.");

        _db.TeacherSubjectAssignments.Add(new TeacherSubjectAssignment
        {
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId
        });
        await _db.SaveChangesAsync();
        return Ok();
    }
    [HttpPost("assign-teacher-class")]
    public async Task<IActionResult> AssignTeacherToClass(AssignTeacherToClassRequest request)
    {
        var teacher = await _db.Users.FindAsync(request.TeacherId);
        if (teacher is null || teacher.Role != UserRole.Teacher) return BadRequest("Invalid teacher.");

        var subject = await _db.Subjects.FindAsync(request.SubjectId);
        if (subject is null) return NotFound("Subject not found.");

        // guards against a subject/classroom mismatch (e.g. duplicate classroom names)
        if (subject.ClassRoomId != request.ClassRoomId)
            return BadRequest("Selected subject does not belong to the selected classroom.");

        var alreadyAssigned = await _db.TeacherSubjectAssignments
            .AnyAsync(t => t.TeacherId == request.TeacherId && t.SubjectId == request.SubjectId);
        if (alreadyAssigned) return Conflict("Teacher is already assigned to this subject.");

        _db.TeacherSubjectAssignments.Add(new TeacherSubjectAssignment
        {
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId
        });
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("assign-student-class")]
    public async Task<IActionResult> AssignStudentClass(UpdateStudentClassRequest request)
    {
        var student = await _db.Users.FindAsync(request.StudentId);
        if (student is null || student.Role != UserRole.Student) return BadRequest("Invalid student.");

        student.ClassId = request.ClassRoomId;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Select(u => new { u.Id, u.FullName, u.Email, Role = u.Role.ToString(), u.ClassId })
            .ToListAsync();
        return Ok(users);
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