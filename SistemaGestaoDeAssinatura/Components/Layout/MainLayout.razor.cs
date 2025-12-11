using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

namespace SistemaGestaoDeAssinatura.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    //[Inject]
    //protected UsuariosRepository Usuario { get; set; }

    //[Inject]
    //protected DialogService DialogService { get; set; }

    //[CascadingParameter]
    //public Task<AuthenticationState> authenticationState { get; set; }
    //[Inject]
    //private IServiceProvider ServiceProvider { get; set; }
    //[Inject]
    //protected RecargaRepository _recargaRepository { get; set; } = default!;
    //[Inject]
    //protected NotificationService NotificationService { get; set; } = default!;
    //[Inject]
    //protected EnviaEmail _email { get; set; } = default!;
    //[Inject]
    //protected ProcessamentoRepository _processamentoRepository { get; set; } = default!;
    private bool sidebarExpanded = true;

    private int UsuarioId;
    private string re = "";
    private string nome = "";
    private MenuItemDisplayStyle DisplayStyle = MenuItemDisplayStyle.IconAndText;
    private bool carregando;

    private bool isAdmin;
    private bool isRH;
    private bool isFrota;
    private static Task TaskProcessmaento = null;

    private string nomeESobrenome
    {
        get
        {
            var separado = nome.Split(" ");
            return $"{separado[0]} {separado[^1]}";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                //var authState = await authenticationState;
                //UsuarioId = authState.GetUserId();
                //var usuario = await Usuario.GetByIdTemp(UsuarioId);
                //re = usuario?.UserName ?? "";
                //nome = usuario?.Nome ?? "";
                //isAdmin = authState.GetAdmin();
                //isRH = authState.GetRH();
                //isFrota = authState.GetFrota();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                //_ = new GravarLog(ex, GetType().Name);
                //await DialogService.OpenAsync<_MesagemErro>("Erro", new Dictionary<string, object> { { "Mensagem", ex } }, new DialogOptions { Width = "55vw", Height = "60vh" });
            }

            StateHasChanged();
        }
    }

    private string backgroudcolor
    {
        get
        {
            var tamanho = sidebarExpanded ? "width:190px" : "width:70px";

            return $"z-index: 2; {tamanho}; overflow: hidden;";
        }
    }

    private void SidebarToggleClick(bool Expanded)
    {
        sidebarExpanded = Expanded;
        DisplayStyle = sidebarExpanded ? MenuItemDisplayStyle.IconAndText : MenuItemDisplayStyle.Icon;

        StateHasChanged();
    }

    public void SetCarregando(bool value)
    {
        try
        {
            carregando = value;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            //var id = Guid.NewGuid().ToString();
            //DialogService.OpenAsync<_MesagemErro>("Erro", new Dictionary<string, object> { { "ID", id }, { "Mensagem", ex } }, new DialogOptions { Width = "55vw", Height = "60vh" }).GetAwaiter().GetResult();
        }
    }
    public async Task processar()
    {
        //if (TaskProcessmaento == null && !ExecutarProcessamento.EmExecucao)
        //{
        //    TaskProcessmaento = Task.Run(async () =>
        //    {
        //        using (var scope = ServiceProvider.CreateScope())
        //        {
        //            var notificacaoRepo = scope.ServiceProvider.GetRequiredService<ExecutarProcessamento>();
        //            await notificacaoRepo.Start();
        //            //var regraRepo = scope.ServiceProvider.GetRequiredService<RegrasRepository>();

        //            TaskProcessmaento = null;
        //        }
        //    });

        //    Notificacao("Iniciado o processamento", NotificationSeverity.Success);
        //}
        //else
        //{
        //    Notificacao("Já existe um processamento em andamento", NotificationSeverity.Error);
        //}
    }
    private async Task EnviaEmail()
    {
        //var centroDeCustos = await _processamentoRepository.GetRegras();
        //var processamento = await _processamentoRepository.GetProximoProcessamento();
        //foreach (var centro in centroDeCustos)
        //{
        //    try
        //    {
        //        EmailCotaExtra email = new();
        //        email.Regra = centro.Nome;
        //        email.Quantidade = processamento.ProcessamentoCombustivel.Where(x => centro.CentroDeCustos.Any(z => z.Nome == x.CentroDeCusto) && x.Status == StatusCombustivel.Pendente).Count();
        //        email.ValorAJustificar = processamento.ProcessamentoCombustivel.Where(x => centro.CentroDeCustos.Any(z => z.Nome == x.CentroDeCusto) && x.Status == StatusCombustivel.Pendente).Sum(x => x.ValorAJustificar);
        //        email.KMAJustificar = processamento.ProcessamentoCombustivel.Where(x => centro.CentroDeCustos.Any(z => z.Nome == x.CentroDeCusto) && x.Status == StatusCombustivel.Pendente).Sum(x => x.KMAJustificar).ToString("0 KM");
        //        email.CentroDeCusto = string.Join(' ', centro.CentroDeCustos.Select(x => x.Nome).ToList());
        //        foreach (var aprovador in centro.FluxoAprovacao.Aprovadores)
        //        {
        //            if (!string.IsNullOrWhiteSpace(aprovador.Usuario.Email))
        //            {
        //                email.Email.Add(aprovador.Usuario.Email.ToString());
        //            }
        //        }
        //        await _email.Enviar($"Email de processamento {email.Regra}", email, true);
        //        if (email.Email?.Count > 0)
        //        {
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        //throw;
        //    }
        //}
    }
    protected void Notificacao(string msg, NotificationSeverity severity)
    {
        //var m = new NotificationMessage { Severity = severity, Summary = msg, Detail = "", Duration = 4000 };
        //NotificationService.Notify(m);
    }
}
