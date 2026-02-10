using System.Text.Json.Serialization;

namespace Domain.DTOs;

public class DeepFaceDetectDTO
{
    public byte[]? Imagem { get; set; }
}

public class FaceDetectadaDTO
{
    [JsonPropertyName("face_index")]
    public int FaceIndex { get; set; }
    
    [JsonPropertyName("region")]
    public Dictionary<string, object>? Regiao { get; set; }
    
    [JsonPropertyName("confidence")]
    public double Confianca { get; set; }
}

public class DeepFaceDetectRespostaDTO
{
    [JsonPropertyName("faces_detectadas")]
    public int FacesDetectadas { get; set; }
    
    [JsonPropertyName("faces")]
    public List<FaceDetectadaDTO> Faces { get; set; } = new();
}
