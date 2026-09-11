namespace project.Models.DTO;

public class AdoptionDto
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public string? AnimalName { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime AdoptionDate { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAdoptionDto
{
    public int AnimalId { get; set; }
    public int UserId { get; set; }
    public DateTime AdoptionDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAdoptionDto
{
    public string? Status { get; set; }
    public string? Notes { get; set; }
}
