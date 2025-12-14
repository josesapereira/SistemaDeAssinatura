using System.Linq.Expressions;
using Domain.Models;
using Domain.DTOs;

namespace Domain.Interfaces.Repository;

public interface IUsuarioRepository : IBaseRepository<Usuario>
{
    Task<Usuario?> GetByUsernameAsync(string username);
    Task<Usuario?> GetByREAsync(string re);
    Task<bool> UsuarioExisteAsync(string re);
}

