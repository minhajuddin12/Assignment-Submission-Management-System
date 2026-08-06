using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Api.DTOs;

public record CreateAssignmentRequest(
    [Required, StringLength(200, MinimumLength = 3)] string Title,
    [Required, StringLength(2000)] string Description,
    [Required] DateTime Deadline,
    [Range(1, 1000)] int MaxMarks,
    [Required] int SubjectId,
    bool PublishNow);

public record UpdateAssignmentRequest(
    [Required, StringLength(200, MinimumLength = 3)] string Title,
    [Required, StringLength(2000)] string Description,
    [Required] DateTime Deadline,
    [Range(1, 1000)] int MaxMarks,
    bool PublishNow);

public record AssignmentResponse(int Id, string Title, string Description, DateTime Deadline, int MaxMarks, string Status, string SubjectName, DateTime CreatedAt);