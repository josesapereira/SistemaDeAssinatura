using Microsoft.AspNetCore.Components;
using Radzen;

namespace SistemaGestaoDeAssinatura.Components
{
    public partial class _MesagemErro
    {
        [Inject]
        public DialogService DialogService { get; set; }
        [Parameter]
        public string Title { get; set; } = "Erro";
        [Parameter]
        public Exception Mensagem { get; set; }
        private void Fechar()
        {
            DialogService.Close();
        }
    }
}
