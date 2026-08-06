using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs;
using AssignmentSystem.Api.Models;
using AssignmentSystem.Api.Services;

namespace AssignmentSystem.Api.Controllers;

[ApiController]
[Route("api/assignments/{assignmentId}/[controller]")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _currentUser;

    public SubmissionsController(AppDbContext db, CurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    // Student: submit an answer. Enforces deadline and one-submission-then-update rule.
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<SubmissionResponse>> Submit(int assignmentId, CreateSubmissionRequest request)
    {
        var assignment = await _db.Assignments.FindAsync(assignmentId);
        if (assignment is null || assignment.Status != AssignmentStatus.Published)
            return NotFound("Assignment not found or not published.");

        var existing = await _db.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == _currentUser.UserId);

        bool isLate = DateTime.UtcNow > assignment.Deadline;

        if (existing is not null)
        {
            // Update before deadline only
            if (DateTime.UtcNow > assignment.Deadline)
                return BadRequest("The deadline has passed; you can no longer update your submission.");

            existing.AnswerText = request.AnswerText;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(await ToResponse(existing));
        }

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = _currentUser.UserId,
            AnswerText = request.AnswerText,
            Status = isLate ? SubmissionStatus.Late : SubmissionStatus.Submitted
        };

        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync();
        return Ok(await ToResponse(submission));
    }

    // Teacher/Admin: view all submissions for this assignment
    [HttpGet]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<List<SubmissionResponse>>> GetAll(int assignmentId)
    {
        var submissions = await _db.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .Where(s => s.AssignmentId == assignmentId)
            .ToListAsync();

        var responses = new List<SubmissionResponse>();
        foreach (var s in submissions) responses.Add(await ToResponse(s));
        return Ok(responses);
    }

    // Student: view their own submission for this assignment
    [HttpGet("mine")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<SubmissionResponse>> GetMine(int assignmentId)
    {
        var submission = await _db.Submissions
            .Include(s => s.Student).Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == _currentUser.UserId);

        if (submission is null) return NotFound();
        return Ok(await ToResponse(submission));
    }

    // Teacher: grade a submission
    [HttpPut("{submissionId}/grade")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> Grade(int assignmentId, int submissionId, GradeSubmissionRequest request)
    {
        var submission = await _db.Submissions.Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.AssignmentId == assignmentId);

        if (submission is null) return NotFound();

        if (request.MarksAwarded < 0 || request.MarksAwarded > submission.Assignment.MaxMarks)
            return BadRequest($"Marks must be between 0 and {submission.Assignment.MaxMarks}.");

        submission.MarksAwarded = request.MarksAwarded;
        submission.TeacherFeedback = request.Feedback;
        submission.Status = SubmissionStatus.Graded;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<SubmissionResponse> ToResponse(Submission s)
    {
        if (s.Student is null) await _db.Entry(s).Reference(x => x.Student).LoadAsync();
        if (s.Assignment is null) await _db.Entry(s).Reference(x => x.Assignment).LoadAsync();

        return new SubmissionResponse(s.Id, s.AnswerText, s.SubmittedAt, s.UpdatedAt, s.Status.ToString(),
            s.MarksAwarded, s.TeacherFeedback, s.Student.FullName, s.Assignment.Title);
    }
    [HttpPut("{submissionId}/status")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> UpdateStatus(int assignmentId, int submissionId, UpdateSubmissionStatusRequest request)
    {
        var submission = await _db.Submissions
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.AssignmentId == assignmentId);

        if (submission is null) return NotFound();

        if (!Enum.TryParse<SubmissionStatus>(request.Status, true, out var parsedStatus))
            return BadRequest("Status must be Submitted, Late, Graded, or ReturnedForRevision.");

        submission.Status = parsedStatus;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}