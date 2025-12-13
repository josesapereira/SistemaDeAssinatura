using Domain.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace SistemaGestaoDeAssinatura.Components.Pages.Autencicacao;

public partial class Validacao2FA : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = null!;

    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = null!;

    private Validacao2FADTO validacao2FA = new();
    private string mensagemErro = string.Empty;
    private bool carregando = false;

    private async Task OnSubmit()
    {
        if (string.IsNullOrEmpty(validacao2FA.Codigo))
        {
            mensagemErro = "Por favor, preencha todos os campos";
            return;
        }

        carregando = true;
        mensagemErro = string.Empty;

        try
        {
            var response = await Http.PostAsJsonAsync("api/Account/Validar2FA", validacao2FA);
            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadFromJsonAsync<RespostaDTO<DadosLogin>>();
                if (resultado != null && resultado.Sucesso)
                {
                    // Recarregar a página para aplicar autenticação completa
                    Navigation.NavigateTo("/usuarios", forceLoad: true);
                }
                else
                {
                    mensagemErro = resultado?.Mensagem ?? "Código inválido";
                }
            }
            else
            {
                mensagemErro = $"Erro: {response.RequestMessage}";
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

