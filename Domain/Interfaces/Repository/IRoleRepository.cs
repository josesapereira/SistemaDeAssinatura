using Domain.Models;

namespace Domain.Interfaces.Repository;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(long id);
    Task<Role> AdicionarAsync(Role role);
}



