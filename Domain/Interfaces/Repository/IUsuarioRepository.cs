using System.Linq.Expressions;
using Domain.Models;
using Domain.DTOs;

namespace Domain.Interfaces.Repository;

public interface IUsuarioRepository
{
    Task<List<Usuario>> GetAllAsync(Expression<Func<Usuario, bool>>? filtro = null);
    Task<Usuario?> GetByIdAsync(Guid id);
    Task<Usuario?> GetByUsernameAsync(string username);
    Task<Usuario> SalvarAsync(Usuario usuario);
    Task<UsuarioDto> ProximaPagina(int pagina, int quantidade, Expression<Func<Usuario, bool>>? filtro = null);
}

