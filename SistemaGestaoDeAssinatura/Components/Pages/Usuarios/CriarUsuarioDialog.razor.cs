using Domain.DTOs;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Radzen;

namespace SistemaGestaoDeAssinatura.Components.Pages.Usuarios;

public partial class CriarUsuarioDialog : ComponentBase
{
    [Parameter]
    public long? UsuarioId { get; set; }

    [Inject]
    public IUsuarioService UsuarioService { get; set; } = null!;

    [Inject]
    public IRegistroAbilityRepository RegistroAbilityRepository { get; set; } = null!;

    [Inject]
    public IRoleRepository RoleRepository { get; set; } = null!;

    [Inject]
    public DialogService DialogService { get; set; } = null!;

    [Inject]
    public NotificationService NotificationService { get; set; } = null!;

    private UsuarioDTO model = new();
    private List<Role> roles = new();
    private string nomeSelecionado = string.Empty;
    private IFormFile? arquivoSelecionado;
    private string? arquivoAtual;
    private bool modoEdicao => UsuarioId.HasValue;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CarregarRoles();

            if (modoEdicao && UsuarioId.HasValue)
            {
                await CarregarUsuarioParaEdicao();
            }
        }
    }

    private async Task CarregarRoles()
    {
        roles = await RoleRepository.GetAllAsync();
        StateHasChanged();
    }

    private async Task CarregarUsuarioParaEdicao()
    {
        if (!UsuarioId.HasValue) return;

        var usuario = await UsuarioService.GetByIdAsync(UsuarioId.Value);
        if (usuario != null)
        {
            model = usuario;
            StateHasChanged();
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = "Usuário não encontrado"
            });
        }
    }

    private async Task OnREChanged()
    {
        if (!string.IsNullOrWhiteSpace(model.UserName))
        {
            var registro = await RegistroAbilityRepository.GetByREAsync(model.UserName);
            if (registro != null)
            {
                model.Nome = registro.Nome;
            }
            else
            {
                model.Nome = string.Empty;
            }
        }
        else
        {
            nomeSelecionado = string.Empty;
        }
        
        StateHasChanged();
    }

    private async Task OnSubmit()
    {
        if (string.IsNullOrEmpty(model.UserName))
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = "UserName é obrigatório"
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
            //model.ArquivoUpload = arquivoSelecionado;
        }

        RespostaDTO<object> resultado;

        resultado = await UsuarioService.SalveUsuarioAsync(model);

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
    private async Task UploadRepitidas(UploadChangeEventArgs upload)
    {
        try
        {
            //var convert = new ConvertCSVToClass();
            if (!upload.Files.Any()) return;

            //await carregamentoCompleto.LoadOnOff(true, upload.Files.Count(), 0);
            foreach (var arquivo in upload.Files)
            {
                try
                {
                    if (arquivo == null) return;
                    string nomeNovo = Guid.NewGuid().ToString() + Path.GetExtension(arquivo.Name);
                    var stream = arquivo.OpenReadStream(20240000);
                    MemoryStream memoryStream = new();
                    await stream.CopyToAsync(memoryStream);
                    model.ArquivoUpload = memoryStream.ToArray();
                    model.NomeDoArquivo = nomeNovo;
                    //(20240000)
                    //var arquivo = await reader();

                }
                catch (Exception ex)
                {
        
                }
              

            }
          

        }
        catch (Exception ex)
        {

        }
    }
}

