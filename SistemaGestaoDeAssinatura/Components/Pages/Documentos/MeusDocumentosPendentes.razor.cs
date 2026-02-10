using Domain.DTOs;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using Domain.Models;
using Domain.Enums;
using System.Security.Cryptography;
using Infraestrutura.Contexto;

namespace SistemaGestaoDeAssinatura.Components.Pages.Documentos;

public partial class MeusDocumentosPendentes : ComponentBase
{
    [Inject]
    public IDocumentoService DocumentoService { get; set; } = null!;

    [Inject]
    public IUsuarioRepository UsuarioRepository { get; set; } = null!;

    [Inject]
    public DialogService DialogService { get; set; } = null!;

    [Inject]
    public NotificationService NotificationService { get; set; } = null!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = null!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    public IDeepFaceService DeepFaceService { get; set; } = null!;

    [Inject]
    public IDocumentoRepository DocumentoRepository { get; set; } = null!;

    [Inject]
    public IFileStorageService FileStorageService { get; set; } = null!;

    [Inject]
    public AppDbContext DbContext { get; set; } = null!;

    private RadzenDataGrid<DocumentoDTO> grid = new();
    private List<DocumentoDTO> documentos = new();
    private long? usuarioId;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ObterUsuarioLogado();
            if (usuarioId.HasValue)
            {
                await Pesquisar();
            }
        }
    }

    private async Task ObterUsuarioLogado()
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState?.User?.Identity?.Name != null)
            {
                var usuario = await UsuarioRepository.GetByUsernameAsync(authState.User.Identity.Name);
                if (usuario != null)
                {
                    usuarioId = usuario.Id;
                }
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = $"Erro ao obter usuário logado: {ex.Message}"
            });
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

    private async Task LoadData(LoadDataArgs args)
    {
        if (!usuarioId.HasValue)
        {
            await ObterUsuarioLogado();
            if (!usuarioId.HasValue)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Aviso",
                    Detail = "Não foi possível identificar o usuário logado"
                });
                return;
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

        var resultado = await DocumentoService.ListarDocumentosPendentesAsync(
            usuarioId.Value,
            orderBy: null,
            ascending: false,
            pagina: pagina,
            quantidade: quantidade);

        if (resultado.Sucesso && resultado.Dados != null)
        {
            documentos = resultado.Dados.Itens;

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

    private async Task VisualizarDocumento(DocumentoDTO documento)
    {
        var resultado = await DialogService.OpenAsync<DocumentoDialog>("Visualizar Documento",
            new Dictionary<string, object> { { "DocumentoId", documento.Id }, { "ModoVisualizacao", true } },
            new DialogOptions { Width = "1200px", Height = "auto", Resizable = true, Draggable = true });
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

    private async Task AssinarComReconhecimentoFacial(DocumentoDTO documento)
    {
        try
        {
            var resultado = await DialogService.OpenAsync<AssinaturaReconhecimentoFacialDialog>(
                "Assinatura com Reconhecimento Facial",
                new Dictionary<string, object> 
                { 
                    { "DocumentoId", documento.Id },
                    { "UsuarioId", usuarioId.Value }
                },
                new DialogOptions 
                { 
                    Width = "600px", 
                    Height = "auto", 
                    Resizable = true, 
                    Draggable = true 
                });

            if (resultado == true)
            {
                // Recarregar lista de documentos
                await Pesquisar();
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Sucesso",
                    Detail = "Documento assinado com sucesso!"
                });
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = $"Erro ao assinar documento: {ex.Message}"
            });
        }
    }
}
