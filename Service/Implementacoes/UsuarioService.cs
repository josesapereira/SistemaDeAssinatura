using AutoMapper;
using Domain.DTOs;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using QRCoder;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Service.Implementacoes;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRegistroAbilityRepository _registroAbilityRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorageService;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IRegistroAbilityRepository registroAbilityRepository,
        IRoleRepository roleRepository,
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        IMapper mapper,
        IEmailService emailService,
        IFileStorageService fileStorageService)
    {
        _usuarioRepository = usuarioRepository;
        _registroAbilityRepository = registroAbilityRepository;
        _roleRepository = roleRepository;
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
    }

    public async Task<bool> ValidarSenhaPadraoAsync(string senha)
    {
        return senha == "123456";
    }

    public async Task<RespostaDTO<DadosLogin>> AutenticarAsync(LoginDTO loginDTO)
    {
        var resposta = new RespostaDTO<DadosLogin>();

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

        var claim = new Claim("TemporaryAuth", "true");
        var existingClaim = await _userManager.GetClaimsAsync(usuario);
        if (!existingClaim.Any(c => c.Type == "TemporaryAuth"))
        {
            await _userManager.AddClaimAsync(usuario, claim);
        }
        await _signInManager.SignInAsync(usuario, isPersistent: false);
        // Verificar se é senha padrão e primeiro acesso
        var isSenhaPadrao = await ValidarSenhaPadraoAsync(loginDTO.Senha);
        if (isSenhaPadrao && usuario.PrimeiroAcesso && !usuario.DoisFatoresAtivo)
        {
            // Autenticação temporária para primeiro acesso
            // Adicionar claim temporário ao usuário

            resposta.Sucesso = true;
            resposta.Mensagem = "Primeiro acesso detectado. Redirecionando para alteração de senha.";
            resposta.Dados = new DadosLogin { TipoAutenticacao = "Temporaria", RedirectTo = "alterar-senha" };
            return resposta;
        }

        // Se 2FA está ativo, requer validação
        if (usuario.DoisFatoresAtivo)
        {
            resposta.Sucesso = true;
            resposta.Mensagem = "2FA ativo. Redirecionando para validação.";
            resposta.Dados = new DadosLogin { TipoAutenticacao = "2FA", RedirectTo = "validar-2fa", Username = usuario.UserName };
            return resposta;
        }

        // Login normal
        await _signInManager.SignInAsync(usuario, isPersistent: true);
        resposta.Sucesso = true;
        resposta.Mensagem = "Login realizado com sucesso";
        resposta.Dados = new DadosLogin { TipoAutenticacao = "Completa", RedirectTo = "ativar-2fa" };
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
        await _usuarioRepository.AtualizarAsync(usuario);

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
        usuario.TwoFactorEnabled = true;
        await _usuarioRepository.AtualizarAsync(usuario);

        // Remover claim temporário se existir
        var temporaryClaim = (await _userManager.GetClaimsAsync(usuario))
            .FirstOrDefault(c => c.Type == "TemporaryAuth");
        if (temporaryClaim != null)
        {
            await _userManager.RemoveClaimAsync(usuario, temporaryClaim);
        }

        // Fazer login completo após ativar 2FA
        await _signInManager.SignInAsync(usuario, isPersistent: false);

        resposta.Sucesso = true;
        resposta.Mensagem = "2FA ativado com sucesso";
        resposta.Dados = new { RedirectTo = "/usuarios" };
        return resposta;
    }

    public async Task<RespostaDTO<DadosLogin>> Validar2FAAsync(Validacao2FADTO validacao2FADTO)
    {
        var resposta = new RespostaDTO<DadosLogin>();

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
        var temporaryClaim = (await _userManager.GetClaimsAsync(usuario))
    .FirstOrDefault(c => c.Type == "TemporaryAuth");
        if (temporaryClaim != null)
        {
            await _userManager.RemoveClaimAsync(usuario, temporaryClaim);
        }


        // Login completo após validar 2FA
        await _signInManager.SignInAsync(usuario, isPersistent: false);

        resposta.Sucesso = true;
        resposta.Mensagem = "2FA validado com sucesso";
        resposta.Dados = new DadosLogin { RedirectTo = "/usuarios" };
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

    public async Task<RespostaDTO<object>> SalveUsuarioAsync(UsuarioDTO dto)
    {
        var resposta = new RespostaDTO<object>();

        // Validar se UserName existe em RegistroAbility
        var registroAbility = await _registroAbilityRepository.GetByREAsync(dto.UserName);
        if (registroAbility == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "UserName não encontrado no RegistroAbility";
            resposta.Erros.Add("UserName não encontrado no RegistroAbility");
            return resposta;
        }
        if (dto.Id != 0)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);
            if (usuario == null)
            {
                resposta.Sucesso = false;
                resposta.Mensagem = "Usuário não encontrado";
                return resposta;
            }

            if (dto.ArquivoUpload != null && dto.ArquivoUpload.Length > 0)
            {
                try
                {
                    // Excluir arquivo anterior se existir
                    if (dto.NomeDoArquivo != usuario.NomeDoArquivo && !string.IsNullOrWhiteSpace(dto.NomeDoArquivo))
                    {
                        var caminhoArquivo = await _fileStorageService.SalvarArquivoAsync(dto.ArquivoUpload, dto.NomeDoArquivo);
                    }

                }
                catch (Exception ex)
                {
                    resposta.Sucesso = false;
                    resposta.Mensagem = $"Erro ao salvar arquivo: {ex.Message}";
                    return resposta;
                }
            }

            // Usar AutoMapper para atualizar o usuário
            usuario.Email = dto.Email;
            usuario.Ativo = dto.Ativo;
            usuario.Nome = dto.Nome;
            usuario.NomeDoArquivo = dto.NomeDoArquivo;

            var resultado = await _userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
            {
                resposta.Sucesso = false;
                resposta.Mensagem = "Erro ao atualizar usuário";
                resposta.Erros.AddRange(resultado.Errors.Select(e => e.Description));
                return resposta;
            }

            // Atualizar role se fornecido
            await AdicionarRoleAoUsuarioAsync(usuario, dto.RoleId);

            resposta.Sucesso = true;
            resposta.Mensagem = "Usuário atualizado com sucesso";
        }
        else
        {
            // Validar se usuário já existe
            var usuarioExiste = await _usuarioRepository.UsuarioExisteAsync(dto.UserName);
            if (usuarioExiste)
            {
                resposta.Sucesso = false;
                resposta.Mensagem = "Usuário já existe com este UserName";
                resposta.Erros.Add("Usuário já existe com este UserName");
                return resposta;
            }

            // Usar AutoMapper para criar o usuário
            var usuario = _mapper.Map<Usuario>(dto);
            usuario.PrimeiroAcesso = true;
            usuario.DoisFatoresAtivo = true;
            usuario.TwoFactorEnabled = true;

            var resultado = await _userManager.CreateAsync(usuario, "123456");

            if (dto.ArquivoUpload != null && dto.ArquivoUpload.Length > 0)
            {
                try
                {
                    var caminhoArquivo = await _fileStorageService.SalvarArquivoAsync(dto.ArquivoUpload, dto.NomeDoArquivo);
                }
                catch (Exception ex)
                {
                    resposta.Sucesso = false;
                    resposta.Mensagem = $"Erro ao salvar arquivo: {ex.Message}";
                    return resposta;
                }
            }

            if (!resultado.Succeeded)
            {
                resposta.Sucesso = false;
                resposta.Mensagem = "Erro ao criar usuário";
                resposta.Erros.AddRange(resultado.Errors.Select(e => e.Description));
                return resposta;
            }

            // Adicionar role se fornecido
            await AdicionarRoleAoUsuarioAsync(usuario, dto.RoleId);
        }

        resposta.Sucesso = true;
        resposta.Mensagem = "Usuário criado com sucesso";
        return resposta;
    }

    public async Task<RespostaDTO<object>> ResetarSenhaAsync(string re)
    {
        var resposta = new RespostaDTO<object>();

        var usuario = await _usuarioRepository.GetByREAsync(re);
        if (usuario == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário não encontrado";
            return resposta;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var resultado = await _userManager.ResetPasswordAsync(usuario, token, "123456");

        if (!resultado.Succeeded)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Erro ao resetar senha";
            resposta.Erros.AddRange(resultado.Errors.Select(e => e.Description));
            return resposta;
        }

        usuario.PrimeiroAcesso = true;
        await _usuarioRepository.AtualizarAsync(usuario);

        resposta.Sucesso = true;
        resposta.Mensagem = "Senha resetada com sucesso para '123456'";
        return resposta;
    }

    public async Task<RespostaDTO<object>> Resetar2FAAsync(string re)
    {
        var resposta = new RespostaDTO<object>();

        var usuario = await _usuarioRepository.GetByREAsync(re);
        if (usuario == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário não encontrado";
            return resposta;
        }
        usuario.TwoFactorEnabled = false;
        usuario.DoisFatoresAtivo = false;
        await _userManager.ResetAuthenticatorKeyAsync(usuario);
        await _usuarioRepository.AtualizarAsync(usuario);

        resposta.Sucesso = true;
        resposta.Mensagem = "2FA resetado com sucesso";
        return resposta;
    }

    public async Task<RespostaDTO<ResultadoPaginado<UsuarioListagemDTO>>> ListarUsuariosAsync(Expression<Func<Usuario, bool>>? filtro = null, Expression<Func<Usuario, object>>? orderBy = null, bool ascending = true, int? pagina = null, int? quantidade = null)
    {
        var resposta = new RespostaDTO<ResultadoPaginado<UsuarioListagemDTO>>();
        resposta.Dados = new();
        var usuarios = await _usuarioRepository.GetAllAsync(filtro, orderBy, ascending, pagina, quantidade);
        var listaDTO = new List<UsuarioListagemDTO>();

        foreach (var usuario in usuarios.Itens)
        {
            listaDTO.Add(new UsuarioListagemDTO
            {
                Id = usuario.Id,
                RE = usuario.UserName ?? "",
                Nome = usuario.Nome ?? "",
                Email = usuario.Email ?? "",
                Ativo = usuario.Ativo,
                Role = usuario.Roles != null && usuario.Roles.Count > 0 ? usuario.Roles[0].Role.Name : "N/A"
            });
        }

        resposta.Sucesso = true;
        resposta.Mensagem = "Lista de usuários carregada com sucesso";
        resposta.Dados.Itens = listaDTO;
        resposta.Dados.TotalItens = usuarios.TotalItens;
        return resposta;
    }

    public async Task<RespostaDTO<UsuarioDetalhesDTO>> ObterUsuarioPorREAsync(string re)
    {
        var resposta = new RespostaDTO<UsuarioDetalhesDTO>();

        var usuario = await _usuarioRepository.GetByREAsync(re);
        if (usuario == null)
        {
            resposta.Sucesso = false;
            resposta.Mensagem = "Usuário não encontrado";
            return resposta;
        }

        var registroAbility = await _registroAbilityRepository.GetByREAsync(re);

        var detalhesDTO = new UsuarioDetalhesDTO
        {
            Id = usuario.Id,
            RE = usuario.UserName ?? "",
            Nome = registroAbility?.Nome ?? "",
            Email = usuario.Email ?? "",
            Ativo = usuario.Ativo,
            ArquivoUpload = usuario.NomeDoArquivo
        };

        resposta.Sucesso = true;
        resposta.Mensagem = "Usuário encontrado";
        resposta.Dados = detalhesDTO;
        return resposta;
    }

    public async Task<UsuarioDTO> GetByIdAsync(long id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            return new UsuarioDTO();

        // Usar AutoMapper para converter Usuario para UsuarioDTO
        var usuarioDTO = _mapper.Map<UsuarioDTO>(usuario);

        // Carregar arquivo se existir
        if (!string.IsNullOrWhiteSpace(usuario.NomeDoArquivo))
        {
            var arquivo = await _fileStorageService.LerArquivoAsync(usuario.NomeDoArquivo);
            usuarioDTO.ArquivoUpload = arquivo ?? Array.Empty<byte>();
        }

        return usuarioDTO;
    }

    /// <summary>
    /// Adiciona ou atualiza a role do usuário apenas se for diferente da atual
    /// </summary>
    private async Task AdicionarRoleAoUsuarioAsync(Usuario usuario, long? roleId)
    {
        if (!roleId.HasValue)
            return;

        var role = await _roleRepository.GetByIdAsync(roleId.Value);
        if (role == null)
            return;

        // Verificar qual role o usuário já tem
        var userRoles = await _userManager.GetRolesAsync(usuario);
        var roleAtual = userRoles.FirstOrDefault();

        // Só adicionar/remover se a role for diferente da atual
        if (roleAtual != role.Name)
        {
            // Remover todas as roles existentes
            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(usuario, userRoles);
            }

            // Adicionar a nova role
            await _userManager.AddToRoleAsync(usuario, role.Name);
        }
    }
}

