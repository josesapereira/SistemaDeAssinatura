using Domain.DTOs;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace SistemaGestaoDeAssinatura.Components.Pages.Usuarios;

public partial class CriarUsuarioDialog : ComponentBase
{
    [Parameter]
    public Guid? UsuarioId { get; set; }

    [Inject]
    public IUsuarioService UsuarioService { get; set; } = null!;

    [Inject]
    public IRegistroAbilityRepository RegistroAbilityRepository { get; set; } = null!;

    [Inject]
    public DialogService DialogService { get; set; } = null!;

    [Inject]
    public NotificationService NotificationService { get; set; } = null!;

    private CriarUsuarioDTO model = new();
    private List<RegistroAbility> registroAbilities = new();
    private string nomeSelecionado = string.Empty;
    private IFormFile? arquivoSelecionado;
    private string? arquivoAtual;
    private bool modoEdicao => UsuarioId.HasValue;

    protected override async Task OnInitializedAsync()
    {
        await CarregarRegistroAbilities();

        if (modoEdicao && UsuarioId.HasValue)
        {
            await CarregarUsuarioParaEdicao();
        }
    }

    private async Task CarregarRegistroAbilities()
    {
        registroAbilities = await RegistroAbilityRepository.GetAllAsync();
    }

    private async Task CarregarUsuarioParaEdicao()
    {
        if (!UsuarioId.HasValue) return;

        var usuarios = await UsuarioService.ListarUsuariosAsync();
        if (usuarios.Sucesso && usuarios.Dados != null)
        {
            var usuario = usuarios.Dados.FirstOrDefault(u => u.Id == UsuarioId.Value);
            if (usuario != null)
            {
                var detalhes = await UsuarioService.ObterUsuarioPorREAsync(usuario.RE);
                if (detalhes.Sucesso && detalhes.Dados != null)
                {
                    model.RE = detalhes.Dados.RE;
                    model.Email = detalhes.Dados.Email;
                    model.Ativo = detalhes.Dados.Ativo;
                    nomeSelecionado = detalhes.Dados.Nome;
                    arquivoAtual = detalhes.Dados.ArquivoUpload;

                    // Buscar o RegistroAbility correspondente
                    var registro = await RegistroAbilityRepository.GetByREAsync(model.RE);
                    if (registro != null)
                    {
                        nomeSelecionado = registro.Nome;
                    }
                }
            }
        }
    }

    private async Task OnREChanged(object? value)
    {
        if (value is string re && !string.IsNullOrEmpty(re))
        {
            var registro = await RegistroAbilityRepository.GetByREAsync(re);
            if (registro != null)
            {
                nomeSelecionado = registro.Nome;
            }
        }
        else
        {
            nomeSelecionado = string.Empty;
        }
    }

    private async Task OnSubmit()
    {
        if (string.IsNullOrEmpty(model.RE))
        {
            NotificationService.Notify(new NotificationMessage 
            { 
                Severity = NotificationSeverity.Error, 
                Summary = "Erro", 
                Detail = "RE é obrigatório" 
            });
            return;
        }

        if (string.IsNullOrEmpty(model.Email))
        {
            NotificationService.Notify(new NotificationMessage 
            { 
                Severity = NotificationSeverity.Error, 
                Summary = "Erro", 
                Detail = "Email é obrigatório" 
            });
            return;
        }

        // Adicionar arquivo ao modelo se foi selecionado
        if (arquivoSelecionado != null)
        {
            model.Arquivo = arquivoSelecionado;
        }

        RespostaDTO<object> resultado;

        if (modoEdicao && UsuarioId.HasValue)
        {
            var atualizarDTO = new AtualizarUsuarioDTO
            {
                Email = model.Email,
                Ativo = model.Ativo,
                Arquivo = model.Arquivo
            };
            resultado = await UsuarioService.AtualizarUsuarioAsync(UsuarioId.Value, atualizarDTO);
        }
        else
        {
            resultado = await UsuarioService.CriarUsuarioAsync(model);
        }

        if (resultado.Sucesso)
        {
            NotificationService.Notify(new NotificationMessage 
            { 
                Severity = NotificationSeverity.Success, 
                Summary = "Sucesso", 
                Detail = resultado.Mensagem 
            });
            DialogService.Close(true);
        }
        else
        {
            NotificationService.Notify(new NotificationMessage 
            { 
                Severity = NotificationSeverity.Error, 
                Summary = "Erro", 
                Detail = resultado.Mensagem 
            });
        }
    }

    private void Cancelar()
    {
        DialogService.Close();
    }
}

