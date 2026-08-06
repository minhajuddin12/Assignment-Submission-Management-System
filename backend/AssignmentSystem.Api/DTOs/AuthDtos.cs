using System.ComponentModel.DataAnnotations;

namespace AssignmentSystem.Api.DTOs;

public record RegisterRequest(
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, StringLength(100, MinimumLength = 6)] string Password,
    [Required] string Role,
    int? ClassId);

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);

public record AuthResponse(string Token, string FullName, string Email, string Role);