namespace Domain.DTOs;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? NomeDaFotoRegistrada { get; set; }
    public bool PrimeiroAcesso { get; set; }
    public bool DoisFatoresAtivo { get; set; }
    public int TotalItens { get; set; }
    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
}



