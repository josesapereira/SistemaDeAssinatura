using Domain.DTOs;
using Domain.Interfaces.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Radzen.Blazor;

namespace SistemaGestaoDeAssinatura.Components.Pages.Usuarios;

public partial class ListaUsuarios : ComponentBase
{
    [Inject]
    public IUsuarioService UsuarioService { get; set; } = null!;

    [Inject]
    public DialogService DialogService { get; set; } = null!;

    [Inject]
    public NotificationService NotificationService { get; set; } = null!;

    private RadzenDataGrid<UsuarioListagemDTO>? grid;
    private List<UsuarioListagemDTO> usuarios = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CarregarUsuarios();
        }
    }

    private async Task CarregarUsuarios()
    {
        var resultado = await UsuarioService.ListarUsuariosAsync();
        if (resultado.Sucesso && resultado.Dados != null)
        {
            usuarios = resultado.Dados;
            StateHasChanged();
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

    private async Task AbrirDialogCriar()
    {
        var resultado = await DialogService.OpenAsync<CriarUsuarioDialog>("Criar Usuário", 
            new Dictionary<string, object>(), 
            new DialogOptions { Width = "700px", Height = "auto" });

        if (resultado != null && resultado is bool && (bool)resultado)
        {
            await CarregarUsuarios();
        }
    }

    private async Task AbrirDialogEditar(UsuarioListagemDTO usuario)
    {
        
        var resultado = await DialogService.OpenAsync<CriarUsuarioDialog>("Editar Usuário", 
            new Dictionary<string, object> { { "UsuarioId", usuario.Id } }, 
            new DialogOptions { Width = "700px", Height = "auto" });

        if (resultado != null && resultado is bool && (bool)resultado)
        {
            await CarregarUsuarios();
        }
    }

    private async Task ResetarSenha(string re)
    {
        var confirmar = await DialogService.Confirm("Deseja realmente resetar a senha deste usuário para '123456'?", 
            "Resetar Senha", new ConfirmOptions { OkButtonText = "Sim", CancelButtonText = "Não" });

        if (confirmar == true)
        {
            var resultado = await UsuarioService.ResetarSenhaAsync(re);
            if (resultado.Sucesso)
            {
                NotificationService.Notify(new NotificationMessage 
                { 
                    Severity = NotificationSeverity.Success, 
                    Summary = "Sucesso", 
                    Detail = resultado.Mensagem 
                });
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
    }

    private async Task Resetar2FA(string re)
    {
        var confirmar = await DialogService.Confirm("Deseja realmente resetar o 2FA deste usuário?", 
            "Resetar 2FA", new ConfirmOptions { OkButtonText = "Sim", CancelButtonText = "Não" });

        if (confirmar == true)
        {
            var resultado = await UsuarioService.Resetar2FAAsync(re);
            if (resultado.Sucesso)
            {
                NotificationService.Notify(new NotificationMessage 
                { 
                    Severity = NotificationSeverity.Success, 
                    Summary = "Sucesso", 
                    Detail = resultado.Mensagem 
                });
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
    }
}

