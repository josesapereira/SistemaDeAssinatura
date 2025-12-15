using System.Linq.Expressions;
using Domain.Models;
using Domain.DTOs;

namespace Domain.Interfaces.Repository;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(long id);
    Task<ResultadoPaginado<Usuario>> GetAllAsync(Expression<Func<Usuario, bool>>? filtro = null, Expression<Func<Usuario, object>>? orderBy = null, bool ascending = true, int? pagina = null, int? quantidade = null);
    Task<int> CountAsync(Expression<Func<Usuario, bool>>? filtro = null);
    Task<Usuario> AdicionarAsync(Usuario entity);
    Task<Usuario> AtualizarAsync(Usuario entity);
    Task<Usuario?> GetByUsernameAsync(string username);
    Task<Usuario?> GetByREAsync(string re);
    Task<bool> UsuarioExisteAsync(string re);
}

