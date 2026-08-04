namespace AssignmentSystem.Api.DTOs;

public record RegisterRequest(string FullName, string Email, string Password, string Role, int? ClassId);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, string FullName, string Email, string Role);