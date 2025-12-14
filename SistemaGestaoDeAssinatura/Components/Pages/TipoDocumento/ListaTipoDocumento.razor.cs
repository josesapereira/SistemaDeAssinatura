using System.Linq.Expressions;
using Domain.DTOs;
using Domain.Interfaces.Service;
using Domain.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace SistemaGestaoDeAssinatura.Components.Pages.TipoDocumento;

public partial class ListaTipoDocumento : ComponentBase
{
    [Inject]
    public ITipoDocumentoService TipoDocumentoService { get; set; } = null!;

    [Inject]
    public DialogService DialogService { get; set; } = null!;

    [Inject]
    public NotificationService NotificationService { get; set; } = null!;

    private RadzenDataGrid<TipoDocumentoDTO> grid = new();
    private List<TipoDocumentoDTO> tiposDocumento = new();

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
            OrderBy = "Nome"
        });
    }

    private async Task LoadData(LoadDataArgs args)
    {
        Expression<Func<Domain.Models.TipoDocumento, bool>>? filtro = null;
        Expression<Func<Domain.Models.TipoDocumento, object>>? orderBy = null;
        bool ascending = true;
        int? pagina = null;
        int? quantidade = null;

        // Construir filtro se houver
        if (args.Filters != null && args.Filters.Any())
        {
            // Implementar filtros se necessário
        }

        // Construir ordenação
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            var propertyName = args.OrderBy.Split(' ').FirstOrDefault();
            if (!string.IsNullOrEmpty(propertyName))
            {
                ascending = !args.OrderBy.Contains(" desc", StringComparison.OrdinalIgnoreCase);

                orderBy = propertyName.ToLower() switch
                {
                    "nome" => (Expression<Func<Domain.Models.TipoDocumento, object>>)(t => t.Nome),
                    "descricao" => (Expression<Func<Domain.Models.TipoDocumento, object>>)(t => t.Descricao),
                    "ativo" => (Expression<Func<Domain.Models.TipoDocumento, object>>)(t => t.Ativo),
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

        var resultado = await TipoDocumentoService.ListarAsync(filtro, orderBy, ascending, pagina, quantidade);
        if (resultado.Sucesso && resultado.Dados != null)
        {
            tiposDocumento = resultado.Dados.Itens;
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

        StateHasChanged();
    }

    private async Task AbrirDialogCriar()
    {
        var resultado = await DialogService.OpenAsync<CriarTipoDocumentoDialog>("Criar Tipo de Documento",
            new Dictionary<string, object>(),
            new DialogOptions { Width = "600px", Height = "auto" });

        if (resultado != null && resultado is bool && (bool)resultado)
        {
            if (grid != null)
            {
                await grid.Reload();
            }
        }
    }

    private async Task AbrirDialogEditar(TipoDocumentoDTO tipoDocumento)
    {
        var resultado = await DialogService.OpenAsync<CriarTipoDocumentoDialog>("Editar Tipo de Documento",
            new Dictionary<string, object> { { "TipoDocumentoId", tipoDocumento.Id } },
            new DialogOptions { Width = "600px", Height = "auto" });

        if (resultado != null && resultado is bool && (bool)resultado)
        {
            if (grid != null)
            {
                await grid.Reload();
            }
        }
    }
}

