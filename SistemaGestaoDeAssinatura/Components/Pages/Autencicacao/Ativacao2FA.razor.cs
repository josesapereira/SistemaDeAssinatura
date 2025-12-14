using Domain.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace SistemaGestaoDeAssinatura.Components.Pages.Autencicacao;

public partial class Ativacao2FA : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = null!;

    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = null!;

    private Ativacao2FADTO? ativacao2FA;
    private string mensagemErro = string.Empty;
    private bool carregando = false;
    private bool carregandoQRCode = true;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated ?? true)
        {
            Navigation.NavigateTo("/", forceLoad: true);
            return;
        }

        await CarregarQRCode();
    }

    private async Task CarregarQRCode()
    {
        carregandoQRCode = true;
        try
        {
            var response = await Http.PostAsJsonAsync("api/account/Ativar2FA", new Ativacao2FADTO());
            var resultado = await response.Content.ReadFromJsonAsync<RespostaDTO<Ativacao2FADTO>>();

            if (resultado != null && resultado.Sucesso && resultado.Dados != null)
            {
                ativacao2FA = resultado.Dados;
            }
            else
            {
                mensagemErro = resultado?.Mensagem ?? "Erro ao gerar QR Code";
            }
        }
        catch (Exception ex)
        {
            mensagemErro = $"Erro: {ex.Message}";
        }
        finally
        {
            carregandoQRCode = false;
        }
    }

    private async Task CopiarCodigo()
    {
        if (ativacao2FA != null && !string.IsNullOrEmpty(ativacao2FA.CodigoManual))
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", ativacao2FA.CodigoManual);
            // Mostrar notificação de sucesso (pode usar RadzenNotification se disponível)
        }
    }

    private async Task OnSubmit()
    {
        if (ativacao2FA == null || string.IsNullOrEmpty(ativacao2FA.CodigoValidacao))
        {
            mensagemErro = "Por favor, digite o código de validação";
            return;
        }

        carregando = true;
        mensagemErro = string.Empty;

        try
        {
            var response = await Http.PostAsJsonAsync("api/account/Ativar2FA", ativacao2FA);
            var resultado = await response.Content.ReadFromJsonAsync<RespostaDTO<object>>();

            if (resultado != null && resultado.Sucesso)
            {
                // Recarregar a página para aplicar autenticação completa
                Navigation.NavigateTo("/home", forceLoad: true);
            }
            else
            {
                mensagemErro = resultado?.Mensagem ?? "Erro ao ativar 2FA";
            }
        }
        catch (Exception ex)
        {
            mensagemErro = $"Erro: {ex.Message}";
        }
        finally
        {
            carregando = false;
        }
    }
}

