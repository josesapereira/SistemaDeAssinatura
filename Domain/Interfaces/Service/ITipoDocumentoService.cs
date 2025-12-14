using System.Linq.Expressions;
using Domain.DTOs;
using Domain.Models;

namespace Domain.Interfaces.Service;

public interface ITipoDocumentoService
{
    Task<RespostaDTO<object>> SalvarAsync(TipoDocumentoDTO dto);
    Task<RespostaDTO<ResultadoPaginado<TipoDocumentoDTO>>> ListarAsync(
        Expression<Func<TipoDocumento, bool>>? filtro = null,
        Expression<Func<TipoDocumento, object>>? orderBy = null,
        bool ascending = true,
        int? pagina = null,
        int? quantidade = null);
    Task<RespostaDTO<TipoDocumentoDTO>> ObterPorIdAsync(Guid id);
}

