using Domain.Enums;

namespace Domain.DTOs;

public class DocumentoDTO
{
    public Guid Id { get; set; }
    public Guid TipoDeDocumentoId { get; set; }
    public string TipoDeDocumento { get; set; } = null!;
    public DateTime DataInclusao { get; set; }
    public long UsuarioInclusaoId { get; set; }
    public string UsuarioInclusao { get; set; } = null!;
    public string NomeDoArquivo { get; set; } = string.Empty;
    public StatusDocumento StatusDocumento { get; set; } = StatusDocumento.Aguardando_assinatura;
    public byte[]? Arquivo { get; set; }

    public List<AssinanteDTO> Assinantes { get; set; } = new();
    public List<AssinaturaDTO> Assinaturas { get; set; } = new();

    public string StatusDocumentoTexto
    {
        get
        {
            return StatusDocumento.ToString().Replace("_"," ");
        }
    }
}

