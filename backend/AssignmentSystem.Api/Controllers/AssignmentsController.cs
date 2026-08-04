using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs;
using AssignmentSystem.Api.Models;
using AssignmentSystem.Api.Services;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _currentUser;

    public AssignmentsController(AppDbContext db, CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    // Teacher/Admin: create a new assignment (draft or published)
    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<AssignmentResponse>> Create(CreateAssignmentRequest request)
    {
        var subject = await _db.Subjects.FindAsync(request.SubjectId);
        if (subject is null) return NotFound("Subject not found.");

        var assignment = new Assignment
        {
            Title = request.Title,
            Description = request.Description,
            Deadline = request.Deadline,
            MaxMarks = request.MaxMarks,
            SubjectId = request.SubjectId,
            CreatedByTeacherId = _currentUser.UserId,
            Status = request.PublishNow ? AssignmentStatus.Published : AssignmentStatus.Draft
        };

        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync();

        return Ok(ToResponse(assignment, subject.Name));
    }

    // Student: see only published assignments for their own class.
    // Teacher/Admin: see all assignments (including drafts) they're relevant to.
    [HttpGet]
    public async Task<ActionResult<List<AssignmentResponse>>> GetAll()
    {
        IQueryable<Assignment> query = _db.Assignments.Include(a => a.Subject);

        if (_currentUser.Role == "Student")
        {
            var student = await _db.Users.FindAsync(_currentUser.UserId);
            query = query.Where(a =>
                a.Status == AssignmentStatus.Published &&
                a.Subject.ClassRoomId == student!.ClassId);
        }
        else if (_currentUser.Role == "Teacher")
        {
            query = query.Where(a => a.CreatedByTeacherId == _currentUser.UserId);
        }
        // Admin sees everything — no filter.

        var assignments = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return Ok(assignments.Select(a => ToResponse(a, a.Subject.Name)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssignmentResponse>> GetById(int id)
    {
        var assignment = await _db.Assignments.Include(a => a.Subject).FirstOrDefaultAsync(a => a.Id == id);
        if (assignment is null) return NotFound();

        if (_currentUser.Role == "Student")
        {
            var student = await _db.Users.FindAsync(_currentUser.UserId);
            if (assignment.Status != AssignmentStatus.Published || assignment.Subject.ClassRoomId != student!.ClassId)
                return Forbid();
        }

        return Ok(ToResponse(assignment, assignment.Subject.Name));
    }

    // Teacher/Admin: update. Only the assignment's creator (or Admin) may edit it.
    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> Update(int id, UpdateAssignmentRequest request)
    {
        var assignment = await _db.Assignments.FindAsync(id);
        if (assignment is null) return NotFound();

        if (_currentUser.Role == "Teacher" && assignment.CreatedByTeacherId != _currentUser.UserId)
            return Forbid();

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.Deadline = request.Deadline;
        assignment.MaxMarks = request.MaxMarks;
        assignment.Status = request.PublishNow ? AssignmentStatus.Published : AssignmentStatus.Draft;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var assignment = await _db.Assignments.FindAsync(id);
        if (assignment is null) return NotFound();

        if (_currentUser.Role == "Teacher" && assignment.CreatedByTeacherId != _currentUser.UserId)
            return Forbid();

        _db.Assignments.Remove(assignment);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static AssignmentResponse ToResponse(Assignment a, string subjectName) =>
        new(a.Id, a.Title, a.Description, a.Deadline, a.MaxMarks, a.Status.ToString(), subjectName, a.CreatedAt);
}