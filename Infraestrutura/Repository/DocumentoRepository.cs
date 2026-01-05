using Domain.Interfaces.Repository;
using Domain.Models;
using Infraestrutura.Contexto;

namespace Infraestrutura.Repository;

public class DocumentoRepository : BaseRepository<Documento>, IDocumentoRepository
{
    public DocumentoRepository(AppDbContext context) : base(context)
    {
    }
}

