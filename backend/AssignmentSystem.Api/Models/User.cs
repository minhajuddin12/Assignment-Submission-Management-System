namespace AssignmentSystem.Api.Models;

public enum UserRole
{
    Admin,
    Teacher,
    Student
}

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // If Role == Student, which class they belong to
    public int? ClassId { get; set; }
    public ClassRoom? ClassRoom { get; set; }
}