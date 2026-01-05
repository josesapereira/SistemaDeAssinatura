using Domain.DTOs;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace SistemaGestaoDeAssinatura.Components.Pages.Documentos;

public partial class SelecionarUsuarioDialog : ComponentBase
{
    [Parameter]
    public List<UsuarioDTO> Usuarios { get; set; } = new();

    [Inject]
    public DialogService DialogService { get; set; } = null!;

    private UsuarioDTO? usuarioSelecionado;

    private void Confirmar()
    {
        if (usuarioSelecionado != null)
        {
            DialogService.Close(usuarioSelecionado);
        }
    }

    private void Cancelar()
    {
        DialogService.Close();
    }
}

