using Microsoft.AspNetCore.Components;
using Radzen;

namespace SistemaGestaoDeAssinatura.Components.Pages;

public partial class ModalObservacao : ComponentBase
{
    [Inject]
    public DialogService DialogService { get; set; } = null!;

    [Parameter]
    public string Title { get; set; } = "Informe o texto";

    [Parameter]
    public bool Obrigatorio { get; set; }

    [Parameter]
    public string Observacao { get; set; } = "";

    [Parameter]
    public bool ReadOnly { get; set; }

    private string textoFormField => ReadOnly ? "Observação" : Obrigatorio ? "Observação (obrigatória)" : "Observação";

    private string textoCancelar => ReadOnly ? "Fechar" : "Cancelar";

    private void Confirmar()
    {
        if (Obrigatorio && string.IsNullOrEmpty(Observacao))
        {
            DialogService.Alert("Observação é obrigatória.", "Alerta");
            return;
        }

        DialogService.Close(Observacao);
    }

    private void Cancelar()
    {
        DialogService.Close();
    }
}

