using Microsoft.AspNetCore.Http;

namespace Domain.DTOs;

public class CriarUsuarioDTO
{
    public string RE { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public IFormFile? Arquivo { get; set; }
}

