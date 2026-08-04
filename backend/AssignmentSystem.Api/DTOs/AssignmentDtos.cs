namespace AssignmentSystem.Api.DTOs;

public record CreateAssignmentRequest(string Title, string Description, DateTime Deadline, int MaxMarks, int SubjectId, bool PublishNow);

public record UpdateAssignmentRequest(string Title, string Description, DateTime Deadline, int MaxMarks, bool PublishNow);

public record AssignmentResponse(int Id, string Title, string Description, DateTime Deadline, int MaxMarks, string Status, string SubjectName, DateTime CreatedAt);