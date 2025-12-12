using Microsoft.AspNetCore.Http;

namespace Domain.DTOs;

public class AtualizarUsuarioDTO
{
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public IFormFile? Arquivo { get; set; }
}

