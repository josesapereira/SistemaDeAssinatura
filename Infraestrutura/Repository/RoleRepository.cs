using Domain.Models;
using Domain.Interfaces.Repository;
using Infraestrutura.Contexto;
using Infraestrutura.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infraestrutura.Repository;

public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public new async Task<Role?> GetByIdAsync(long id)
    {
        return await _context.Roles.FindAsync(id);
    }
}

