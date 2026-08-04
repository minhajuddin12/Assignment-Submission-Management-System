namespace AssignmentSystem.Api.Models;

public class ClassRoom
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  // e.g. "Grade 10 - Section A"

    public ICollection<User> Students { get; set; } = new List<User>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}