using Microsoft.EntityFrameworkCore;
using AssignmentSystem.Api.Models;

namespace AssignmentSystem.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherSubjectAssignment> TeacherSubjectAssignments => Set<TeacherSubjectAssignment>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Submission>()
            .HasIndex(s => new { s.AssignmentId, s.StudentId })
            .IsUnique(); // one submission per student per assignment (updated, not duplicated)

        modelBuilder.Entity<User>()
            .HasOne(u => u.ClassRoom)
            .WithMany(c => c.Students)
            .HasForeignKey(u => u.ClassId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}