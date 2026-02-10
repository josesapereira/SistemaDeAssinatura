using System.Text.Json.Serialization;

namespace Domain.DTOs;

public class DeepFaceVerifyDTO
{
    public byte[]? Imagem1 { get; set; }
    public byte[]? Imagem2 { get; set; }
    public string? Modelo { get; set; } = "VGG-Face";
    public string? MetricaDistancia { get; set; } = "cosine";
}

public class DeepFaceVerifyRespostaDTO
{
    [JsonPropertyName("verificado")]
    public bool Verificado { get; set; }
    
    [JsonPropertyName("distancia")]
    public double Distancia { get; set; }
    
    [JsonPropertyName("limiar")]
    public double Limiar { get; set; }
    
    [JsonPropertyName("modelo")]
    public string Modelo { get; set; } = string.Empty;
    
    [JsonPropertyName("metrica")]
    public string Metrica { get; set; } = string.Empty;
}
