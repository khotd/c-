using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using project.Data;
using project.Models.Entities;
using project.Repositories;
using Xunit;

namespace project.Tests.Repositories;

public class AnimalRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AnimalRepository _repository;

    public AnimalRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", "Host=localhost;Port=5432;Database=test;Username=test;Password=test" }
        });
        var configuration = configBuilder.Build();

        _repository = new AnimalRepository(_context, configuration);
        
        SeedData();
    }

    private void SeedData()
    {
        var shelter = new Shelter
        {
            Id = 1,
            Name = "Test Shelter",
            Address = "Test Address",
            Capacity = 100,
            CreatedAt = DateTime.UtcNow
        };
        _context.Shelters.Add(shelter);

        var animal = new Animal
        {
            Id = 1,
            Name = "Test Animal",
            Species = "Dog",
            Breed = "Labrador",
            Age = 3,
            Gender = "Male",
            Status = "Available",
            ShelterId = 1,
            ArrivalDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _context.Animals.Add(animal);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsAnimal_WhenExists()
    {
        var result = await _repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Animal", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_CreatesAnimal()
    {
        var animal = new Animal
        {
            Name = "New Animal",
            Species = "Cat",
            Breed = "Persian",
            Age = 2,
            Gender = "Female",
            Status = "Available",
            ShelterId = 1,
            ArrivalDate = DateTime.UtcNow
        };

        var result = await _repository.AddAsync(animal);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("New Animal", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAnimal()
    {
        var animal = await _repository.GetByIdAsync(1);
        animal!.Name = "Updated Name";

        var result = await _repository.UpdateAsync(animal);

        Assert.Equal("Updated Name", result.Name);
        var updated = await _repository.GetByIdAsync(1);
        Assert.Equal("Updated Name", updated!.Name);
    }

    [Fact]
    public async Task DeleteAsync_DeletesAnimal_WhenExists()
    {
        var result = await _repository.DeleteAsync(1);

        Assert.True(result);
        var deleted = await _repository.GetByIdAsync(1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        var result = await _repository.DeleteAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task GetByShelterIdAsync_ReturnsAnimals_ForShelter()
    {
        var animal2 = new Animal
        {
            Name = "Animal 2",
            Species = "Dog",
            Breed = "Golden Retriever",
            Age = 4,
            Gender = "Male",
            Status = "Available",
            ShelterId = 1,
            ArrivalDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(animal2);

        var result = await _repository.GetByShelterIdAsync(1);

        Assert.NotNull(result);
        Assert.True(result.Count >= 2);
        Assert.All(result, a => Assert.Equal(1, a.ShelterId));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
