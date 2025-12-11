using AutoMapper;
using Domain.DTOs;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using QRCoder;
using System.Security.Claims;

namespace Service.Implementacoes;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        IMapper mapper,
        IEmailService emailService)
    {
        _usuarioRepository = usuarioRepository;
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task<bool> ValidarSenhaPadraoAsync(string senha)
    {
        return senha == "123456";
    }

    public async Task<RespostaDTO<object>> AutenticarAsync(LoginDTO loginDTO)
    {
        var resposta = new RespostaDTO<object>();

        var usuario = await _usuarioRepository.GetByUsernameAsync(loginDTO.Username);
        if (usuario == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário ou senha inválidos";
            return resposta;
        }

        var senhaValida = await _userManager.CheckPasswordAsync(usuario, loginDTO.Senha);
        if (!senhaValida)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário ou senha inválidos";
            return resposta;
        }

        // Verificar se é senha padrão e primeiro acesso
        var isSenhaPadrao = await ValidarSenhaPadraoAsync(loginDTO.Senha);
        if (isSenhaPadrao && usuario.PrimeiroAcesso && !usuario.DoisFatoresAtivo)
        {
            // Autenticação temporária para primeiro acesso
            // Adicionar claim temporário ao usuário
            var claim = new Claim("TemporaryAuth", "true");
            var existingClaim = await _userManager.GetClaimsAsync(usuario);
            if (!existingClaim.Any(c => c.Type == "TemporaryAuth"))
            {
                await _userManager.AddClaimAsync(usuario, claim);
            }
            await _signInManager.SignInAsync(usuario, isPersistent: false);
            resposta.Sucesso = true;
            resposta.Mensagem = "Primeiro acesso detectado. Redirecionando para alteração de senha.";
            resposta.Dados = new { TipoAutenticacao = "Temporaria", RedirectTo = "/alterar-senha" };
            return resposta;
        }

        // Se 2FA está ativo, requer validação
        if (usuario.DoisFatoresAtivo)
        {
            resposta.Sucesso = true;
            resposta.Mensagem = "2FA ativo. Redirecionando para validação.";
            resposta.Dados = new { TipoAutenticacao = "2FA", RedirectTo = "/validar-2fa", Username = usuario.UserName };
            return resposta;
        }

        // Login normal
        await _signInManager.SignInAsync(usuario, isPersistent: true);
        resposta.Sucesso = true;
        resposta.Mensagem = "Login realizado com sucesso";
        resposta.Dados = new { TipoAutenticacao = "Completa", RedirectTo = "/home" };
        return resposta;
    }

    public async Task<RespostaDTO<object>> AlterarSenhaAsync(string username, AlterarSenhaDTO alterarSenhaDTO)
    {
        var resposta = new RespostaDTO<object>();

        if (alterarSenhaDTO.NovaSenha != alterarSenhaDTO.ConfirmarNovaSenha)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "As senhas não coincidem";
            resposta.Erros.Add("As senhas não coincidem");
            return resposta;
        }

        if (alterarSenhaDTO.NovaSenha.Length < 6)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "A senha deve ter no mínimo 6 caracteres";
            resposta.Erros.Add("A senha deve ter no mínimo 6 caracteres");
            return resposta;
        }

        var usuario = await _usuarioRepository.GetByUsernameAsync(username);
        if (usuario == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário não encontrado";
            return resposta;
        }

        // Validar senha atual apenas se não for a padrão
        var isSenhaPadrao = await ValidarSenhaPadraoAsync(alterarSenhaDTO.SenhaAtual);
        if (!isSenhaPadrao)
        {
            var senhaValida = await _userManager.CheckPasswordAsync(usuario, alterarSenhaDTO.SenhaAtual);
            if (!senhaValida)
            {
                resposta.Sucesso = false;
                resposta.Mensagem = "Senha atual inválida";
                return resposta;
            }
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var result = await _userManager.ResetPasswordAsync(usuario, token, alterarSenhaDTO.NovaSenha);

        if (!result.Succeeded)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Erro ao alterar senha";
            resposta.Erros.AddRange(result.Errors.Select(e => e.Description));
            return resposta;
        }

        usuario.PrimeiroAcesso = false;
        await _usuarioRepository.SalvarAsync(usuario);

        resposta.Sucesso = true;
        resposta.Mensagem = "Senha alterada com sucesso";
        resposta.Dados = new { RedirectTo = "/ativar-2fa" };
        return resposta;
    }

    public async Task<RespostaDTO<Ativacao2FADTO>> GerarQRCode2FAAsync(string username)
    {
        var resposta = new RespostaDTO<Ativacao2FADTO>();

        var usuario = await _usuarioRepository.GetByUsernameAsync(username);
        if (usuario == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário não encontrado";
            return resposta;
        }

        // Gerar chave secreta para 2FA
        var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(usuario);
        if (string.IsNullOrEmpty(authenticatorKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(usuario);
            authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(usuario);
        }

        // Formatar código manual (XXXX-XXXX)
        var codigoManual = FormatCodigoManual(authenticatorKey);

        // Gerar QR Code
        var qrCodeText = $"otpauth://totp/{usuario.UserName}?secret={authenticatorKey}&issuer=SistemaGestaoDeAssinatura";
        var qrCodeBase64 = GerarQRCodeBase64(qrCodeText);

        var ativacao2FA = new Ativacao2FADTO
        {
            QRCodeBase64 = qrCodeBase64,
            CodigoManual = codigoManual
        };

        resposta.Sucesso = true;
        resposta.Dados = ativacao2FA;
        return resposta;
    }

    public async Task<RespostaDTO<object>> Ativar2FAAsync(string username, string codigoValidacao)
    {
        var resposta = new RespostaDTO<object>();

        var usuario = await _usuarioRepository.GetByUsernameAsync(username);
        if (usuario == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário não encontrado";
            return resposta;
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            usuario,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            codigoValidacao);

        if (!isValid)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Código de validação inválido";
            return resposta;
        }

        usuario.DoisFatoresAtivo = true;
        await _usuarioRepository.SalvarAsync(usuario);

        // Remover claim temporário se existir
        var temporaryClaim = (await _userManager.GetClaimsAsync(usuario))
            .FirstOrDefault(c => c.Type == "TemporaryAuth");
        if (temporaryClaim != null)
        {
            await _userManager.RemoveClaimAsync(usuario, temporaryClaim);
        }

        // Fazer login completo após ativar 2FA
        await _signInManager.SignInAsync(usuario, isPersistent: true);

        resposta.Sucesso = true;
        resposta.Mensagem = "2FA ativado com sucesso";
        resposta.Dados = new { RedirectTo = "/home" };
        return resposta;
    }

    public async Task<RespostaDTO<object>> Validar2FAAsync(Validacao2FADTO validacao2FADTO)
    {
        var resposta = new RespostaDTO<object>();

        var usuario = await _usuarioRepository.GetByUsernameAsync(validacao2FADTO.Username);
        if (usuario == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário não encontrado";
            return resposta;
        }

        if (!usuario.DoisFatoresAtivo)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "2FA não está ativo para este usuário";
            return resposta;
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            usuario,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            validacao2FADTO.Codigo);

        if (!isValid)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Código 2FA inválido";
            return resposta;
        }

        // Login completo após validar 2FA
        await _signInManager.SignInAsync(usuario, isPersistent: true);

        resposta.Sucesso = true;
        resposta.Mensagem = "2FA validado com sucesso";
        resposta.Dados = new { RedirectTo = "/home" };
        return resposta;
    }

    private string FormatCodigoManual(string? key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 8)
            return key ?? string.Empty;

        return $"{key.Substring(0, 4)}-{key.Substring(4, 4)}";
    }

    private string GerarQRCodeBase64(string texto)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);
        return Convert.ToBase64String(qrCodeBytes);
    }
}

