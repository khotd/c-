namespace project.Models.Entities;

public class Adoption
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public int UserId { get; set; }
    public DateTime AdoptionDate { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Animal Animal { get; set; } = null!;
    public User User { get; set; } = null!;
}
