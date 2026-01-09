namespace project.Models.DTO;

public class ShelterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Capacity { get; set; }
    public int CurrentAnimals { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateShelterDto
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Capacity { get; set; }
}

public class UpdateShelterDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? Capacity { get; set; }
}
