using Domain.DTOs;
using Domain.Interfaces.Service;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace SistemaGestaoDeAssinatura.Components.Pages.TipoDocumento;

public partial class CriarTipoDocumentoDialog : ComponentBase
{
    [Parameter]
    public Guid? TipoDocumentoId { get; set; }

    [Inject]
    public ITipoDocumentoService TipoDocumentoService { get; set; } = null!;

    [Inject]
    public DialogService DialogService { get; set; } = null!;

    [Inject]
    public NotificationService NotificationService { get; set; } = null!;

    private TipoDocumentoDTO model = new();
    private bool modoEdicao => TipoDocumentoId.HasValue;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (modoEdicao && TipoDocumentoId.HasValue)
            {
                await CarregarTipoDocumentoParaEdicao();
            }
        }
    }

    private async Task CarregarTipoDocumentoParaEdicao()
    {
        if (!TipoDocumentoId.HasValue) return;

        var resultado = await TipoDocumentoService.ObterPorIdAsync(TipoDocumentoId.Value);
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
                Detail = resultado.Mensagem ?? "Tipo de documento não encontrado"
            });
        }
    }

    private async Task OnSubmit()
    {
        if (string.IsNullOrWhiteSpace(model.Nome))
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Erro",
                Detail = "Nome é obrigatório"
            });
            return;
        }

        var resultado = await TipoDocumentoService.SalvarAsync(model);

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

