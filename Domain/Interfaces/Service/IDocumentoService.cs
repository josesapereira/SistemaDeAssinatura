using System.Linq.Expressions;
using Domain.DTOs;
using Domain.Models;

namespace Domain.Interfaces.Service;

public interface IDocumentoService
{
    Task<RespostaDTO<object>> SalvarAsync(DocumentoDTO dto);
    Task<RespostaDTO<ResultadoPaginado<DocumentoDTO>>> ListarAsync(
        Expression<Func<Documento, bool>>? filtro = null,
        Expression<Func<Documento, object>>? orderBy = null,
        bool ascending = true,
        int? pagina = null,
        int? quantidade = null);
    Task<RespostaDTO<DocumentoDTO>> ObterPorIdAsync(Guid id);
    Task<RespostaDTO<ResultadoPaginado<DocumentoDTO>>> ListarDocumentosPendentesAsync(
        long usuarioId,
        Expression<Func<Documento, object>>? orderBy = null,
        bool ascending = true,
        int? pagina = null,
        int? quantidade = null);
}

