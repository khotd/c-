using project.Models.DTO;
using project.Models.Entities;

namespace project.Services.Interfaces;

public interface IShelterService
{
    Task<List<ShelterDto>> GetAllAsync();
    Task<ShelterDto?> GetByIdAsync(int id);
    Task<ShelterDto> CreateAsync(CreateShelterDto dto, string? currentUserRole);
    Task<ShelterDto> UpdateAsync(int id, UpdateShelterDto dto, string? currentUserRole);
    Task<bool> DeleteAsync(int id, string? currentUserRole);
}
