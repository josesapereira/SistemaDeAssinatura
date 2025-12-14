using System.Linq.Expressions;
using Domain.DTOs;
using Domain.Interfaces.Service;
using Domain.Models;
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

    private RadzenDataGrid<UsuarioListagemDTO> grid = new();
    private List<UsuarioListagemDTO> usuarios = new();
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Pesquisar();
        }
    }
    public async Task Pesquisar()
    {
        await LoadData(new LoadDataArgs
        {
            Skip = 0,
            Top = 100,
            OrderBy = "re"            
            
        });
    }
    private async Task LoadData(LoadDataArgs args)
    {
        Expression<Func<Usuario, bool>>? filtro = null;
        Expression<Func<Usuario, object>>? orderBy = null;
        bool ascending = true;
        int? pagina = null;
        int? quantidade = null;

        // Construir filtro se houver
        if (args.Filters != null && args.Filters.Any())
        {

        }

        // Construir ordenação
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            var propertyName = args.OrderBy.Split(' ').FirstOrDefault();
            if (!string.IsNullOrEmpty(propertyName))
            {
                ascending = !args.OrderBy.Contains(" desc", StringComparison.OrdinalIgnoreCase);
                
                // Mapear propriedades do DTO para propriedades do Model
                // Nota: "nome" vem de RegistroAbility, então não pode ser ordenado diretamente no banco
                orderBy = propertyName.ToLower() switch
                {
                    "re" => (Expression<Func<Usuario, object>>)(u => u.UserName ?? string.Empty),
                    "email" => (Expression<Func<Usuario, object>>)(u => u.Email ?? string.Empty),
                    "ativo" => (Expression<Func<Usuario, object>>)(u => u.Ativo),
                    _ => null
                };
            }
        }

        // Paginação
        if (args.Skip.HasValue && args.Top.HasValue && args.Top.Value > 0)
        {
            pagina = args.Skip.Value / args.Top.Value;
            quantidade = args.Top.Value;
        }

        var resultado = await UsuarioService.ListarUsuariosAsync(filtro, orderBy, ascending, pagina, quantidade);
        if (resultado.Sucesso && resultado.Dados != null)
        {
            usuarios = resultado.Dados.Itens;
            if (grid != null)
            {
                grid.Count = resultado.Dados.TotalItens;
            }
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
        //grid.Reload();\
        StateHasChanged();
    }

    private async Task AbrirDialogCriar()
    {
        var resultado = await DialogService.OpenAsync<CriarUsuarioDialog>("Criar Usuário", 
            new Dictionary<string, object>(), 
            new DialogOptions { Width = "900px", Height = "auto" });

        if (resultado != null && resultado is bool && (bool)resultado)
        {
            if (grid != null)
            {
                await grid.Reload();
            }
        }
    }

    private async Task AbrirDialogEditar(UsuarioListagemDTO usuario)
    {
        
        var resultado = await DialogService.OpenAsync<CriarUsuarioDialog>("Editar Usuário", 
            new Dictionary<string, object> { { "UsuarioId", usuario.Id } }, 
            new DialogOptions { Width = "700px", Height = "auto" });

        if (resultado != null && resultado is bool && (bool)resultado)
        {
            if (grid != null)
            {
                await grid.Reload();
            }
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

