namespace Domain.DTOs;

public class UsuarioListagemDTO
{
    public long Id { get; set; }
    public string RE { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public string Role { get; set; }
}

