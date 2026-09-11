namespace project.Models.DTO;

public class AnimalDto
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
    public string? ShelterName { get; set; }
    public DateTime ArrivalDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAnimalDto
{
    public string Name { get; set; } = null!;
    public string Species { get; set; } = null!;
    public string Breed { get; set; } = null!;
    public int Age { get; set; }
    public string Gender { get; set; } = null!;
    public string Status { get; set; } = "Available";
    public string? Description { get; set; }
    public int ShelterId { get; set; }
    public DateTime ArrivalDate { get; set; }
}

public class UpdateAnimalDto
{
    public string? Name { get; set; }
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public int? ShelterId { get; set; }
}

public class AnimalFilterDto
{
    public string? Search { get; set; }
    public string? Species { get; set; }
    public string? Status { get; set; }
    public int? ShelterId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
