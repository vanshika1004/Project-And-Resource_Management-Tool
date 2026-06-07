using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ForcePasswordChange { get; set; } = true;

    // Navigation Properties

    public Employee? Employee { get; set; }

    public ICollection<Project> ManagedProjects { get; set; }
        = new List<Project>();
}