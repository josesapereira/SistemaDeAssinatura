using Domain.DTOs;
using Domain.Enums;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

namespace SistemaGestaoDeAssinatura.Components.Pages.Documentos;

public partial class DocumentoDialog : ComponentBase
{
    [Parameter]
    public Guid? DocumentoId { get; set; }

    [Parameter]
    public bool ModoVisualizacao { get; set; } = false;

    [Inject]
    public IDocumentoService DocumentoService { get; set; } = null!;

    [Inject]
    public ITipoDocumentoService TipoDocumentoService { get; set; } = null!;

    [Inject]
    public IRegistroAbilityRepository RegistroAbilityRepository { get; set; } = null!;

    [Inject]
    public IUsuarioRepository UsuarioRepository { get; set; } = null!;

    [Inject]
    public IUsuarioService UsuarioService { get; set; } = null!;

    [Inject]
    public DialogService DialogService { get; set; } = null!;

    [Inject]
    public NotificationService NotificationService { get; set; } = null!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private DocumentoDTO model = new();
    private List<TipoDocumentoDTO> tiposDocumento = new();
    private bool modoEdicao => DocumentoId.HasValue && DocumentoId.Value != Guid.Empty;
    private bool modoVisualizacao => ModoVisualizacao;
    private bool temAssinatura => model.Assinaturas != null && model.Assinaturas.Any() && 
                                  model.Assinaturas.Any(a => a.DataDaAssinatura != default);

    private BadgeStyle BadgeStyleStatus
    {
        get
        {
            return model.StatusDocumento switch
            {
                StatusDocumento.Assinado => BadgeStyle.Success,
                StatusDocumento.Documento_revogado => BadgeStyle.Danger,
                StatusDocumento.Documento_cancelado => BadgeStyle.Danger,
                StatusDocumento.Aguardando_assinatura_da_revogacao => BadgeStyle.Warning,
                _ => BadgeStyle.Info
            };
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CarregarTiposDocumento();

            if (modoEdicao && DocumentoId.HasValue)
            {
                await CarregarDocumentoParaEdicao();
            }
            else
            {
                // Inicializar com status padrão
                model.StatusDocumento = StatusDocumento.Aguardando_assinatura;
                
                // Obter usuário atual para UsuarioInclusaoId
                if (AuthenticationState != null)
                {
                    var authState = await AuthenticationState;
                    if (authState?.User?.Identity?.Name != null)
                    {
                        var usuario = await UsuarioRepository.GetByUsernameAsync(authState.User.Identity.Name);
                        if (usuario != null)
                        {
                            model.UsuarioInclusaoId = usuario.Id;
                        }
                    }
                }
            }
        }
    }

    private async Task CarregarTiposDocumento()
    {
        var resultado = await TipoDocumentoService.ListarAsync();
        if (resultado.Sucesso && resultado.Dados != null)
        {
            tiposDocumento = resultado.Dados.Itens.Where(t => t.Ativo).ToList();
            StateHasChanged();
        }
    }

    private async Task CarregarDocumentoParaEdicao()
    {
        if (!DocumentoId.HasValue) return;

        var resultado = await DocumentoService.ObterPorIdAsync(DocumentoId.Value);
        if (resultado.Sucesso && resultado.Dados != null)
        {
            model = resultado.Dados;
            StateHasChanged();
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = resultado.Mensagem ?? "Documento não encontrado"
            });
        }
    }

    private async Task UploadArquivo(UploadChangeEventArgs upload)
    {
        try
        {
            if (!upload.Files.Any()) return;

            foreach (var arquivo in upload.Files)
            {
                if (arquivo == null) return;

                var stream = arquivo.OpenReadStream(20240000);
                MemoryStream memoryStream = new();
                await stream.CopyToAsync(memoryStream);
                model.Arquivo = memoryStream.ToArray();
                model.NomeDoArquivo = arquivo.Name;

                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = $"Erro ao fazer upload do arquivo: {ex.Message}"
            });
        }
    }

    private async Task OnSubmit()
    {
        if (model.TipoDeDocumentoId == Guid.Empty)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = "Tipo de documento é obrigatório"
            });
            return;
        }

        if (model.Arquivo == null || model.Arquivo.Length == 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = "Arquivo é obrigatório"
            });
            return;
        }

        if (model.Assinantes == null || !model.Assinantes.Any())
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = "É necessário informar pelo menos um assinante"
            });
            return;
        }

        var resultado = await DocumentoService.SalvarAsync(model);

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

    private async Task Revogar()
    {
        var confirmar = await DialogService.Confirm(
            "Deseja realmente revogar este documento? Esta ação não pode ser desfeita.",
            "Revogar Documento",
            new ConfirmOptions { OkButtonText = "Sim", CancelButtonText = "Não" });

        if (confirmar == true)
        {
            model.StatusDocumento = StatusDocumento.Documento_revogado;
            var resultado = await DocumentoService.SalvarAsync(model);

            if (resultado.Sucesso)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Sucesso",
                    Detail = "Documento revogado com sucesso"
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
    }

    private async Task AdicionarAssinante()
    {
        // Buscar todos os usuários ativos
        var resultadoUsuarios = await UsuarioRepository.GetAllAsync(
            filtro: u => u.Ativo,
            orderBy: u => u.Nome,
            ascending: true);

        if (resultadoUsuarios == null || !resultadoUsuarios.Itens.Any())
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = "Aviso",
                Detail = "Nenhum usuário ativo encontrado"
            });
            return;
        }

        // Converter para UsuarioDTO
        var usuariosDTO = resultadoUsuarios.Itens.Select(u => new UsuarioDTO
        {
            Id = u.Id,
            UserName = u.UserName ?? string.Empty,
            Nome = u.Nome ?? string.Empty,
            Email = u.Email ?? string.Empty,
            Ativo = u.Ativo
        }).ToList();

        // Filtrar usuários que já estão na lista
        var usuariosDisponiveis = usuariosDTO
            .Where(u => !model.Assinantes.Any(a => a.AssinanteId == u.Id))
            .ToList();

        if (!usuariosDisponiveis.Any())
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = "Aviso",
                Detail = "Todos os usuários já foram adicionados como assinantes"
            });
            return;
        }

        // Abrir dialog para selecionar usuário
        UsuarioDTO usuarioSelecionado = await DialogService.OpenAsync<SelecionarUsuarioDialog>(
            "Adicionar Assinante",
            new Dictionary<string, object> { { "Usuarios", usuariosDisponiveis } },
            new DialogOptions { Width = "500px", Height = "auto" });

        if (usuarioSelecionado != null && usuarioSelecionado is UsuarioDTO usuario)
        {
            // Verificar se já não está na lista (dupla verificação)
            if (model.Assinantes.Any(a => a.AssinanteId == usuario.Id))
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Aviso",
                    Detail = "Este assinante já está na lista"
                });
                return;
            }

            // Adicionar assinante
            model.Assinantes.Add(new AssinanteDTO
            {
                Id = Guid.NewGuid(),
                AssinanteId = usuario.Id,
                UsuarioAssinante = usuario.Nome,
                DocumentoId = model.Id,
                StatusDaAssinatura = StatusDaAssinatura.Pendente,
                DataAssinatura = null
            });

            StateHasChanged();
        }
    }

    private void RemoverAssinante(AssinanteDTO assinante)
    {
        model.Assinantes.Remove(assinante);
        StateHasChanged();
    }

    private void Cancelar()
    {
        DialogService.Close();
    }
}

