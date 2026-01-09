using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using project.Data;
using project.Models.DTO;
using project.Models.Entities;
using project.Repositories.Interfaces;

namespace project.Repositories;

public class AnimalRepository : IAnimalRepository
{
    private readonly AppDbContext _db;
    private readonly string _connectionString;

    public AnimalRepository(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public Task<List<Animal>> GetAllAsync()
    {
        return _db.Animals
            .Include(a => a.Shelter)
            .ToListAsync();
    }

    public Task<Animal?> GetByIdAsync(int id)
    {
        return _db.Animals
            .Include(a => a.Shelter)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Animal> AddAsync(Animal animal)
    {
        animal.CreatedAt = DateTime.UtcNow;
        _db.Animals.Add(animal);
        await _db.SaveChangesAsync();
        return animal;
    }

    public async Task<Animal> UpdateAsync(Animal animal)
    {
        animal.UpdatedAt = DateTime.UtcNow;
        _db.Animals.Update(animal);
        await _db.SaveChangesAsync();
        return animal;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var animal = await _db.Animals.FindAsync(id);
        if (animal == null)
            return false;

        _db.Animals.Remove(animal);
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<List<Animal>> GetByShelterIdAsync(int shelterId)
    {
        return _db.Animals
            .Where(a => a.ShelterId == shelterId)
            .Include(a => a.Shelter)
            .ToListAsync();
    }

    public async Task<PagedResponse<Animal>> GetPagedAsync(AnimalFilterDto filter)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();
        
        try
        {
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                whereClauses.Add("(a.name ILIKE @Search OR a.breed ILIKE @Search OR a.description ILIKE @Search)");
                parameters.Add("Search", $"%{filter.Search}%");
            }
            
            if (!string.IsNullOrWhiteSpace(filter.Species))
            {
                whereClauses.Add("a.species = @Species");
                parameters.Add("Species", filter.Species);
            }
            
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                whereClauses.Add("a.status = @Status");
                parameters.Add("Status", filter.Status);
            }
            
            if (filter.ShelterId.HasValue)
            {
                whereClauses.Add("a.shelter_id = @ShelterId");
                parameters.Add("ShelterId", filter.ShelterId.Value);
            }
            
            var whereClause = whereClauses.Any() 
                ? "WHERE " + string.Join(" AND ", whereClauses)
                : "";
            
            var offset = (filter.Page - 1) * filter.PageSize;
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", filter.PageSize);
            
            var countSql = $@"
                SELECT COUNT(*) 
                FROM animals a
                {whereClause}";
            
            var total = await connection.QuerySingleAsync<int>(countSql, parameters, transaction);
            
            var sql = $@"
                SELECT 
                    a.id AS Id, 
                    a.name AS Name, 
                    a.species AS Species, 
                    a.breed AS Breed, 
                    a.age AS Age, 
                    a.gender AS Gender, 
                    a.status AS Status, 
                    a.description AS Description, 
                    a.shelter_id AS ShelterId, 
                    a.arrival_date AS ArrivalDate, 
                    a.created_at AS CreatedAt, 
                    a.updated_at AS UpdatedAt
                FROM animals a
                {whereClause}
                ORDER BY a.created_at DESC
                LIMIT @PageSize OFFSET @Offset";
            
            var animals = await connection.QueryAsync<Animal>(sql, parameters, transaction);
            
            await transaction.CommitAsync();
            
            return new PagedResponse<Animal>
            {
                Items = animals.ToList(),
                Total = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
