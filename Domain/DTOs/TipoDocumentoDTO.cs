namespace Domain.DTOs;

public class TipoDocumentoDTO
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

