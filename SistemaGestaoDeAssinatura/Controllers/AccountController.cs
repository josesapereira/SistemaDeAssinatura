using Domain.DTOs;
using Domain.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SistemaCotaExtra.Controllers
{
    [Route("api/Account/[action]")]
    public class AccountController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public AccountController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string userName, string password, string redirectUrl = "dashboard")
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new RespostaDTO<object>
                {
                    Sucesso = false,
                    Mensagem = "Dados inválidos",
                    Erros = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var resultado = await _usuarioService.AutenticarAsync(new LoginDTO { Username = userName, Senha = password });
            
            if (!resultado.Sucesso)
            {
                // Retornar erro na query string para a tela de login capturar
                // Não preservar redirectUrl quando há erro
                var errorMessage = Uri.EscapeDataString(resultado.Mensagem ?? "Erro ao fazer login");
                return Redirect($"~/?error={errorMessage}");
            }
            
            string redirecionar = "~/";
            if (resultado.Dados != null)
            {
                var dados = resultado.Dados as DadosLogin;
                if (dados != null && dados.RedirectTo != null)
                {
                    redirecionar = $"~/{dados.RedirectTo}";
                }
            }
            
            return Redirect(redirecionar);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDTO alterarSenhaDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new RespostaDTO<object>
                {
                    Sucesso = false,
                    Mensagem = "Dados inválidos",
                    Erros = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new RespostaDTO<object>
                {
                    Sucesso = false,
                    Mensagem = "Usuário não autenticado"
                });
            }

            var resultado = await _usuarioService.AlterarSenhaAsync(username, alterarSenhaDTO);

            if (!resultado.Sucesso)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Ativar2FA([FromBody] Ativacao2FADTO ativacao2FADTO)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new RespostaDTO<object>
                {
                    Sucesso = false,
                    Mensagem = "Usuário não autenticado"
                });
            }

            // Se já tem código de validação, ativar 2FA
            if (!string.IsNullOrEmpty(ativacao2FADTO.CodigoValidacao))
            {
                var resultado = await _usuarioService.Ativar2FAAsync(username, ativacao2FADTO.CodigoValidacao);
                if (!resultado.Sucesso)
                {
                    return BadRequest(resultado);
                }
                return Ok(resultado);
            }

            // Caso contrário, gerar QRCode
            var qrCodeResultado = await _usuarioService.GerarQRCode2FAAsync(username);
            return Ok(qrCodeResultado);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Validar2FA([FromBody] Validacao2FADTO validacao2FADTO)
        {
            var username = User.Identity?.Name;
            validacao2FADTO.Username = username;
            if (!ModelState.IsValid)
            {
                return BadRequest(new RespostaDTO<DadosLogin>
                {
                    Sucesso = false,
                    Mensagem = "Dados inválidos",
                    Erros = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var resultado = await _usuarioService.Validar2FAAsync(validacao2FADTO);

            if (!resultado.Sucesso)
            {
                return Unauthorized(resultado);
            }

            return Ok(resultado);
        }

    }
}
