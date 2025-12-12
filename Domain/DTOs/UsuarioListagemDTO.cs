namespace Domain.DTOs;

public class UsuarioListagemDTO
{
    public Guid Id { get; set; }
    public string RE { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

