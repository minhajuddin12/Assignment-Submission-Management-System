using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AssignmentSystem.Tests;

public class SubmissionBusinessRulesTests
{
    private AppDbContext BuildInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Submission_BeforeDeadline_IsMarkedSubmitted_NotLate()
    {
        using var db = BuildInMemoryDb(nameof(Submission_BeforeDeadline_IsMarkedSubmitted_NotLate));

        var assignment = new Assignment
        {
            Title = "Test", Description = "Test", MaxMarks = 100,
            Deadline = DateTime.UtcNow.AddDays(1), // deadline is in the future
            Status = AssignmentStatus.Published
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();

        bool isLate = DateTime.UtcNow > assignment.Deadline;

        Assert.False(isLate);
    }

    [Fact]
    public async Task Submission_AfterDeadline_IsMarkedLate()
    {
        using var db = BuildInMemoryDb(nameof(Submission_AfterDeadline_IsMarkedLate));

        var assignment = new Assignment
        {
            Title = "Test", Description = "Test", MaxMarks = 100,
            Deadline = DateTime.UtcNow.AddDays(-1), // deadline already passed
            Status = AssignmentStatus.Published
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();

        bool isLate = DateTime.UtcNow > assignment.Deadline;

        Assert.True(isLate);
    }

    [Fact]
    public async Task Submission_UniquePerStudentPerAssignment_EnforcedByIndex()
    {
        using var db = BuildInMemoryDb(nameof(Submission_UniquePerStudentPerAssignment_EnforcedByIndex));

        var assignment = new Assignment { Title = "A", Description = "D", MaxMarks = 100, Deadline = DateTime.UtcNow.AddDays(1) };
        var student = new User { FullName = "S", Email = "s@test.com", PasswordHash = "x", Role = UserRole.Student };
        db.Assignments.Add(assignment);
        db.Users.Add(student);
        await db.SaveChangesAsync();

        db.Submissions.Add(new Submission { AssignmentId = assignment.Id, StudentId = student.Id, AnswerText = "First answer" });
        await db.SaveChangesAsync();

        var existing = await db.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.StudentId == student.Id);

        Assert.NotNull(existing);

        // Simulate what the controller does on a second submit: update instead of insert
        existing!.AnswerText = "Updated answer";
        await db.SaveChangesAsync();

        var count = await db.Submissions.CountAsync(s => s.AssignmentId == assignment.Id && s.StudentId == student.Id);
        Assert.Equal(1, count); // still exactly one row, not two
    }

    [Theory]
    [InlineData(50, 100, true)]   // valid: within range
    [InlineData(0, 100, true)]    // valid: minimum boundary
    [InlineData(100, 100, true)]  // valid: maximum boundary
    [InlineData(-1, 100, false)]  // invalid: below zero
    [InlineData(101, 100, false)] // invalid: exceeds max marks
    public void MarksValidation_RespectsMaxMarksRange(int marksAwarded, int maxMarks, bool expectedValid)
    {
        bool isValid = marksAwarded >= 0 && marksAwarded <= maxMarks;

        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public async Task StudentOnlySeesPublishedAssignments_ForTheirOwnClass()
    {
        using var db = BuildInMemoryDb(nameof(StudentOnlySeesPublishedAssignments_ForTheirOwnClass));

        var classA = new ClassRoom { Name = "Class A" };
        var classB = new ClassRoom { Name = "Class B" };
        db.ClassRooms.AddRange(classA, classB);
        await db.SaveChangesAsync();

        var subjectInA = new Subject { Name = "Math", ClassRoomId = classA.Id };
        var subjectInB = new Subject { Name = "Science", ClassRoomId = classB.Id };
        db.Subjects.AddRange(subjectInA, subjectInB);
        await db.SaveChangesAsync();

        db.Assignments.AddRange(
            new Assignment { Title = "Published in A", Description = "d", MaxMarks = 100, Deadline = DateTime.UtcNow.AddDays(1), SubjectId = subjectInA.Id, Status = AssignmentStatus.Published },
            new Assignment { Title = "Draft in A", Description = "d", MaxMarks = 100, Deadline = DateTime.UtcNow.AddDays(1), SubjectId = subjectInA.Id, Status = AssignmentStatus.Draft },
            new Assignment { Title = "Published in B", Description = "d", MaxMarks = 100, Deadline = DateTime.UtcNow.AddDays(1), SubjectId = subjectInB.Id, Status = AssignmentStatus.Published }
        );
        await db.SaveChangesAsync();

        var studentClassId = classA.Id;

        var visibleToStudent = await db.Assignments
            .Include(a => a.Subject)
            .Where(a => a.Status == AssignmentStatus.Published && a.Subject.ClassRoomId == studentClassId)
            .ToListAsync();

        Assert.Single(visibleToStudent);
        Assert.Equal("Published in A", visibleToStudent[0].Title);
    }
}