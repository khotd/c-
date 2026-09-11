namespace project.Models.Entities;

public class Animal
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Species { get; set; } = null!;
    public string Breed { get; set; } = null!;
    public int Age { get; set; }
    public string Gender { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Description { get; set; }
    public int ShelterId { get; set; }
    public DateTime ArrivalDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Shelter Shelter { get; set; } = null!;
    public ICollection<Adoption> Adoptions { get; set; } = new List<Adoption>();
    public ICollection<AnimalVolunteer> AnimalVolunteers { get; set; } = new List<AnimalVolunteer>();
}
