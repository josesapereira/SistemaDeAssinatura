using Domain.DTOs;

namespace Domain.Interfaces.Service;

public interface IUsuarioService
{
    Task<RespostaDTO<DadosLogin>> AutenticarAsync(LoginDTO loginDTO);
    Task<RespostaDTO<object>> AlterarSenhaAsync(string username, AlterarSenhaDTO alterarSenhaDTO);
    Task<RespostaDTO<Ativacao2FADTO>> GerarQRCode2FAAsync(string username);
    Task<RespostaDTO<object>> Ativar2FAAsync(string username, string codigoValidacao);
    Task<RespostaDTO<DadosLogin>> Validar2FAAsync(Validacao2FADTO validacao2FADTO);
    Task<bool> ValidarSenhaPadraoAsync(string senha);
    Task<RespostaDTO<object>> CriarUsuarioAsync(CriarUsuarioDTO dto);
    Task<RespostaDTO<object>> AtualizarUsuarioAsync(Guid id, AtualizarUsuarioDTO dto);
    Task<RespostaDTO<object>> ResetarSenhaAsync(string re);
    Task<RespostaDTO<object>> Resetar2FAAsync(string re);
    Task<RespostaDTO<List<UsuarioListagemDTO>>> ListarUsuariosAsync();
    Task<RespostaDTO<UsuarioDetalhesDTO>> ObterUsuarioPorREAsync(string re);
}



