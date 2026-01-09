namespace project.Models.Entities;

public class AnimalVolunteer
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = null!;
    public DateTime AssignedDate { get; set; }
    public DateTime? UnassignedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Animal Animal { get; set; } = null!;
    public User User { get; set; } = null!;
}
