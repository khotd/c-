namespace project.Models.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? Permissions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<Adoption> Adoptions { get; set; } = new List<Adoption>();
    public ICollection<AnimalVolunteer> AnimalVolunteers { get; set; } = new List<AnimalVolunteer>();
}
