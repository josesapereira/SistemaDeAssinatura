namespace Domain.DTOs;

public class RespostaDTO<T>
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public T? Dados { get; set; }
    public List<string> Erros { get; set; } = new();
}



