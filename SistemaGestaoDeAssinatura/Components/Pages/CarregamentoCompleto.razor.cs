using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace SistemaGestaoDeAssinatura.Components.Pages;

public partial class CarregamentoCompleto : ComponentBase
{
    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public int Maximo { get; set; } = 0;

    [Parameter]
    public int Minimo { get; set; } = 0;
}

