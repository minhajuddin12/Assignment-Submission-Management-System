namespace AssignmentSystem.Api.Models;

public enum SubmissionStatus
{
    Submitted,
    Late,
    Graded,
    ReturnedForRevision
}

public class Submission
{
    public int Id { get; set; }
    public string AnswerText { get; set; } = string.Empty;

    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    public int? MarksAwarded { get; set; }
    public string? TeacherFeedback { get; set; }
}