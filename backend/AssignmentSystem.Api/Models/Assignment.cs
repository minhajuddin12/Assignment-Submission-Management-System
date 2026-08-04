namespace AssignmentSystem.Api.Models;

public enum AssignmentStatus
{
    Draft,
    Published
}

public class Assignment
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public int MaxMarks { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public int CreatedByTeacherId { get; set; }
    public User CreatedByTeacher { get; set; } = null!;

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}