using Domain.Models;

namespace Domain.Interfaces.Repository;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role> SalvarAsync(Role role);
}



