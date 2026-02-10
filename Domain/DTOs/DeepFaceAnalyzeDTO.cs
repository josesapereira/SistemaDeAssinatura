using System.Text.Json.Serialization;

namespace Domain.DTOs;

public class DeepFaceAnalyzeDTO
{
    public byte[]? Imagem { get; set; }
    public List<string>? Acoes { get; set; } // ["emotion", "age", "gender", "race"]
}

public class DeepFaceAnalyzeRespostaDTO
{
    [JsonPropertyName("emotion")]
    public string? Emocao { get; set; }
    
    [JsonPropertyName("emotion_scores")]
    public Dictionary<string, double>? EmocaoScores { get; set; }
    
    [JsonPropertyName("age")]
    public int? Idade { get; set; }
    
    [JsonPropertyName("gender")]
    public string? Genero { get; set; }
    
    [JsonPropertyName("gender_scores")]
    public Dictionary<string, double>? GeneroScores { get; set; }
    
    [JsonPropertyName("race")]
    public string? Raca { get; set; }
    
    [JsonPropertyName("race_scores")]
    public Dictionary<string, double>? RacaScores { get; set; }
}
