namespace AssignmentSystem.Api.Models;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int ClassRoomId { get; set; }
    public ClassRoom ClassRoom { get; set; } = null!;

    public ICollection<TeacherSubjectAssignment> TeacherAssignments { get; set; } = new List<TeacherSubjectAssignment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}