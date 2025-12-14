using System.Linq.Expressions;
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
    Task<RespostaDTO<object>> SalveUsuarioAsync(CriarUsuarioDTO dto);
    //Task<RespostaDTO<object>> AtualizarUsuarioAsync(CriarUsuarioDTO dto);
    Task<RespostaDTO<object>> ResetarSenhaAsync(string re);
    Task<RespostaDTO<object>> Resetar2FAAsync(string re);
    Task<RespostaDTO<ResultadoPaginado<UsuarioListagemDTO>>> ListarUsuariosAsync(Expression<Func<Domain.Models.Usuario, bool>>? filtro = null, Expression<Func<Domain.Models.Usuario, object>>? orderBy = null, bool ascending = true, int? pagina = null, int? quantidade = null);
    Task<RespostaDTO<UsuarioDetalhesDTO>> ObterUsuarioPorREAsync(string re);
    Task<CriarUsuarioDTO> GetByIdAsync(Guid id);
}



