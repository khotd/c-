namespace project.Models.Entities;

public class Shelter
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<Animal> Animals { get; set; } = new List<Animal>();
}
