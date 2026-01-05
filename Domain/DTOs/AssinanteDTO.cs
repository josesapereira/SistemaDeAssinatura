using Domain.Enums;

namespace Domain.DTOs;

public class AssinanteDTO
{
    public Guid Id { get; set; }
    public long AssinanteId { get; set; }
    public string UsuarioAssinante { get; set; } = null!;
    public Guid DocumentoId { get; set; }
    public StatusDaAssinatura StatusDaAssinatura { get; set; }
    public DateTime? DataAssinatura { get; set; }
}

