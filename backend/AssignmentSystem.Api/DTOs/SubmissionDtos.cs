namespace AssignmentSystem.Api.DTOs;

public record CreateSubmissionRequest(string AnswerText);

public record GradeSubmissionRequest(int MarksAwarded, string? Feedback);

public record SubmissionResponse(int Id, string AnswerText, DateTime SubmittedAt, DateTime? UpdatedAt, string Status, int? MarksAwarded, string? TeacherFeedback, string StudentName, string AssignmentTitle);