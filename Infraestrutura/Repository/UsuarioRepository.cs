using System.Linq.Expressions;
using Domain.Models;
using Domain.DTOs;
using Domain.Interfaces.Repository;
using Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Infraestrutura.Repository;

public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context)
    {
        //if(context.)
        context.Database.OpenConnectionAsync().GetAwaiter().GetResult();
    }

    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            //.Include(u => u.Roles)
            //.ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == username);
    }



    public async Task<Usuario?> GetByREAsync(string re)
    {
        return await _dbSet
            //.Include(u => u.Roles)
            //.ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == re);
    }

    public async Task<bool> UsuarioExisteAsync(string re)
    {
        return await _dbSet.AnyAsync(u => u.UserName == re);
    }

    //public override async Task<Usuario?> GetByIdAsync(Guid id)
    //{
    //    return await _dbSet
    //        .Include(u => u.Roles)
    //        .ThenInclude(ur => ur.Role)
    //        .FirstOrDefaultAsync(u => u.Id.ToString() == id.ToString());
    //}
}

