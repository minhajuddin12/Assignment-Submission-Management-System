using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Api.DTOs;

public record CreateSubmissionRequest([Required, StringLength(10000, MinimumLength = 1)] string AnswerText);

public record GradeSubmissionRequest([Range(0, 1000)] int MarksAwarded, [StringLength(1000)] string? Feedback);

public record SubmissionResponse(int Id, string AnswerText, DateTime SubmittedAt, DateTime? UpdatedAt, string Status, int? MarksAwarded, string? TeacherFeedback, string StudentName, string AssignmentTitle);