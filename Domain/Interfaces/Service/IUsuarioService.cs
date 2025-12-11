using Domain.DTOs;

namespace Domain.Interfaces.Service;

public interface IUsuarioService
{
    Task<RespostaDTO<object>> AutenticarAsync(LoginDTO loginDTO);
    Task<RespostaDTO<object>> AlterarSenhaAsync(string username, AlterarSenhaDTO alterarSenhaDTO);
    Task<RespostaDTO<Ativacao2FADTO>> GerarQRCode2FAAsync(string username);
    Task<RespostaDTO<object>> Ativar2FAAsync(string username, string codigoValidacao);
    Task<RespostaDTO<object>> Validar2FAAsync(Validacao2FADTO validacao2FADTO);
    Task<bool> ValidarSenhaPadraoAsync(string senha);
}



