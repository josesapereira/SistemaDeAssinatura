using System.Linq.Expressions;
using Domain.DTOs;
using Domain.Enums;
using Domain.Extensions;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace SistemaGestaoDeAssinatura.Components.Pages.Documentos;

public partial class ListaDocumentos : ComponentBase
{
    [Inject]
    public IDocumentoService DocumentoService { get; set; } = null!;

    [Inject]
    public ITipoDocumentoService TipoDocumentoService { get; set; } = null!;

    [Inject]
    public DialogService DialogService { get; set; } = null!;

    [Inject]
    public NotificationService NotificationService { get; set; } = null!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = null!;

    private RadzenDataGrid<DocumentoDTO> grid = new();
    private List<DocumentoDTO> documentos = new();
    private List<TipoDocumentoDTO> tiposDocumento = new();

    // Filtros
    private DateTime? filtroDataInicio;
    private DateTime? filtroDataFim;
    private Guid? filtroTipoDocumentoId;
    private StatusDocumento? filtroStatusDocumento;
    private string filtroNomeAssinante = string.Empty;

    // Lista de status para o dropdown
    private List<StatusDocumentoItem> statusDocumentoList = new()
    {
        new StatusDocumentoItem { Value = null, Text = "Todos" },
        new StatusDocumentoItem { Value = StatusDocumento.Aguardando_assinatura, Text = "Aguardando Assinatura" },
        new StatusDocumentoItem { Value = StatusDocumento.Assinado, Text = "Assinado" },
        new StatusDocumentoItem { Value = StatusDocumento.Aguardando_assinatura_da_revogacao, Text = "Aguardando Assinatura da Revogação" },
        new StatusDocumentoItem { Value = StatusDocumento.Documento_revogado, Text = "Documento Revogado" },
        new StatusDocumentoItem { Value = StatusDocumento.Documento_cancelado, Text = "Documento Cancelado" }
    };

    private class StatusDocumentoItem
    {
        public StatusDocumento? Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CarregarTiposDocumento();
            await Pesquisar();
        }
    }

    private async Task CarregarTiposDocumento()
    {
        var resultado = await TipoDocumentoService.ListarAsync();
        if (resultado.Sucesso && resultado.Dados != null)
        {
            tiposDocumento = resultado.Dados.Itens;
            StateHasChanged();
        }
    }

    public async Task Pesquisar()
    {
        await LoadData(new LoadDataArgs
        {
            Skip = 0,
            Top = 10,
            OrderBy = "DataInclusao desc"
        });
    }

    private async Task Filtrar()
    {
        await Pesquisar();
    }

    private async Task LoadData(LoadDataArgs args)
    {
        Expression<Func<Documento, bool>>? filtro = null;

        // Aplicar filtros
        if (filtroDataInicio.HasValue)
        {
            var dataInicio = filtroDataInicio.Value.Date;
            filtro = filtro.AndAlso(d => d.DataInclusao.Date >= dataInicio);
        }

        if (filtroDataFim.HasValue)
        {
            var dataFim = filtroDataFim.Value.Date.AddDays(1).AddTicks(-1);
            filtro = filtro.AndAlso(d => d.DataInclusao <= dataFim);
        }

        if (filtroTipoDocumentoId.HasValue && filtroTipoDocumentoId.Value != Guid.Empty)
        {
            filtro = filtro.AndAlso(d => d.TipoDeDocumentoId == filtroTipoDocumentoId.Value);
        }

        if (filtroStatusDocumento.HasValue)
        {
            filtro = filtro.AndAlso(d => d.StatusDocumento == filtroStatusDocumento.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtroNomeAssinante))
        {
            // Filtrar por username ou nome do assinante
            // Como é uma busca em relacionamento, vamos fazer após carregar os dados
        }

        Expression<Func<Documento, object>>? orderBy = null;
        bool ascending = true;

        // Construir ordenação
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            var propertyName = args.OrderBy.Split(' ').FirstOrDefault();
            if (!string.IsNullOrEmpty(propertyName))
            {
                ascending = !args.OrderBy.Contains(" desc", StringComparison.OrdinalIgnoreCase);

                orderBy = propertyName.ToLower() switch
                {
                    "tipodedocumento" => (Expression<Func<Documento, object>>)(d => d.TipoDeDocumento.Nome),
                    "datainclusao" => (Expression<Func<Documento, object>>)(d => d.DataInclusao),
                    "usuarioinclusao" => (Expression<Func<Documento, object>>)(d => d.UsuarioInclusao.Nome),
                    _ => (Expression<Func<Documento, object>>)(d => d.DataInclusao)
                };
            }
        }

        // Paginação
        int? pagina = null;
        int? quantidade = null;
        if (args.Skip.HasValue && args.Top.HasValue && args.Top.Value > 0)
        {
            pagina = args.Skip.Value / args.Top.Value;
            quantidade = args.Top.Value;
        }

        var resultado = await DocumentoService.ListarAsync(filtro, orderBy, ascending, pagina, quantidade);

        if (resultado.Sucesso && resultado.Dados != null)
        {
            documentos = resultado.Dados.Itens;

            // Aplicar filtro de nome/username do assinante após carregar
            if (!string.IsNullOrWhiteSpace(filtroNomeAssinante))
            {
                var filtroLower = filtroNomeAssinante.ToLower();
                documentos = documentos.Where(d => 
                    d.Assinantes.Any(a => 
                        a.AssinanteId.ToString().Contains(filtroLower) || 
                        a.UsuarioAssinante.ToLower().Contains(filtroLower)
                    )
                ).ToList();
            }

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
        var resultado = await DialogService.OpenAsync<DocumentoDialog>("Novo Documento",
            new Dictionary<string, object>(),
            new DialogOptions { Width = "1200px", Height = "auto", Resizable = true, Draggable = true });

        if (resultado != null && resultado is bool && (bool)resultado)
        {
            if (grid != null)
            {
                await grid.Reload();
            }
        }
    }

    private async Task VisualizarDocumento(DocumentoDTO documento)
    {
        var resultado = await DialogService.OpenAsync<DocumentoDialog>("Visualizar Documento",
            new Dictionary<string, object> { { "DocumentoId", documento.Id }, { "ModoVisualizacao", true } },
            new DialogOptions { Width = "1200px", Height = "auto", Resizable = true, Draggable = true });
    }

    private async Task EditarDocumento(DocumentoDTO documento)
    {
        var resultado = await DialogService.OpenAsync<DocumentoDialog>("Editar Documento",
            new Dictionary<string, object> { { "DocumentoId", documento.Id } },
            new DialogOptions { Width = "1200px", Height = "auto", Resizable = true, Draggable = true });

        if (resultado != null && resultado is bool && (bool)resultado)
        {
            if (grid != null)
            {
                await grid.Reload();
            }
        }
    }

    private async Task DownloadDocumento(DocumentoDTO documento)
    {
        try
        {
            // Buscar documento completo com arquivo
            var resultado = await DocumentoService.ObterPorIdAsync(documento.Id);
            if (resultado.Sucesso && resultado.Dados != null && resultado.Dados.Arquivo != null)
            {
                // Usar JavaScript para fazer download - passar array de bytes diretamente
                await JSRuntime.InvokeVoidAsync("download", resultado.Dados.NomeDoArquivo, resultado.Dados.Arquivo);
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Aviso",
                    Detail = "Arquivo não encontrado"
                });
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = $"Erro ao fazer download: {ex.Message}"
            });
        }
    }
}

