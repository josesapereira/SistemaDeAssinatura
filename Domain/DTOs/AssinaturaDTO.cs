using Domain.Enums;

namespace Domain.DTOs;

public class AssinaturaDTO
{
    public Guid Id { get; set; }
    public DateTime DataDaAssinatura { get; set; }
    public TipoAssinatura TipoAssinatura { get; set; }
    public Guid AssinanteId { get; set; }
    public string Assinante { get; set; } = null!;
    public string SistemaDeAssinatura { get; set; } = string.Empty;
    public string VersaoSistema { get; set; } = string.Empty;
    public string SistemaOperacional { get; set; } = string.Empty;
    public string IPDaAssinatura { get; set; } = string.Empty;
    public Guid DocumentoId { get; set; }
}

