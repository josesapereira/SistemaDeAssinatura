# DeepFace Service

Serviço Python FastAPI para processamento facial usando DeepFace e MediaPipe, integrado ao Sistema de Assinatura Digital.

## Funcionalidades

- ✅ **Verificação de Identidade**: Compara duas faces para verificar se pertencem à mesma pessoa
- ✅ **Análise de Atributos**: Detecta emoções, idade, gênero e raça
- ✅ **Detecção de Faces**: Identifica e localiza faces em imagens
- ✅ **Prova de Vida (Liveness)**: Valida ações faciais em fotos (piscar, sorrir, virar cabeça)

## Requisitos

- Python 3.8+
- pip

## Instalação

1. Criar ambiente virtual (recomendado):
```bash
python -m venv venv
source venv/bin/activate  # Linux/Mac
# ou
venv\Scripts\activate  # Windows
```

2. Instalar dependências:
```bash
pip install -r requirements.txt
```

**Nota**: A primeira execução pode demorar pois o DeepFace baixa os modelos necessários automaticamente.

## Execução

### Windows (Recomendado)

**Opção 1 - Modo Simples:**
```bash
iniciar-servico.bat
```

**Opção 2 - Modo Desenvolvimento (com reload automático):**
```bash
iniciar-servico-reload.bat
```

**Instalar dependências:**
```bash
instalar-dependencias.bat
```

### Linux/Mac

**Desenvolvimento:**
```bash
python app.py
```

**Produção (com Uvicorn):**
```bash
uvicorn app:app --host 0.0.0.0 --port 8000 --reload
```

O serviço estará disponível em: `http://localhost:8000`

## Endpoints

### Health Check
```
GET /api/deepface/health
```

### Verificação de Identidade
```
POST /api/deepface/verify
```
**Parâmetros:**
- `img1` / `img1_base64`: Primeira imagem (arquivo ou base64)
- `img2` / `img2_base64`: Segunda imagem (arquivo ou base64)
- `model_name`: Modelo a usar (padrão: "VGG-Face")
- `distance_metric`: Métrica de distância (padrão: "cosine")

**Resposta:**
```json
{
  "verificado": true,
  "distancia": 0.25,
  "limiar": 0.4,
  "modelo": "VGG-Face",
  "metrica": "cosine"
}
```

### Análise de Atributos
```
POST /api/deepface/analyze
```
**Parâmetros:**
- `img` / `img_base64`: Imagem (arquivo ou base64)
- `actions`: Ações a analisar, separadas por vírgula (ex: "emotion,age,gender,race")

**Resposta:**
```json
{
  "emotion": "happy",
  "emotion_scores": {...},
  "age": 30,
  "gender": "Man",
  "gender_scores": {...},
  "race": "white",
  "race_scores": {...}
}
```

### Detecção de Faces
```
POST /api/deepface/detect
```
**Parâmetros:**
- `img` / `img_base64`: Imagem (arquivo ou base64)

**Resposta:**
```json
{
  "faces_detectadas": 1,
  "faces": [
    {
      "face_index": 0,
      "region": {...},
      "confidence": 0.99
    }
  ]
}
```

### Prova de Vida - Gerar Instrução
```
POST /api/deepface/liveness/generate-instruction
```
**Parâmetros:**
- `last_instruction`: (opcional) Última instrução gerada (para evitar repetição)

**Resposta:**
```json
{
  "instrucao": "Por favor, pisque os olhos agora",
  "acao": "piscar",
  "timestamp": "2024-01-15T10:30:00"
}
```

### Prova de Vida - Validar
```
POST /api/deepface/liveness/validate
```
**Parâmetros:**
- `img` / `img_base64`: Foto capturada (arquivo ou base64)
- `instruction`: Ação esperada ("piscar", "sorrir", "virar_esquerda", "virar_direita")

**Resposta:**
```json
{
  "aprovado": true,
  "acao_detectada": "piscar",
  "confianca": 0.85,
  "detalhes": {
    "instrucao_solicitada": "piscar",
    "blink_detectado": true,
    "blink_confidence": 0.85,
    "smile_detectado": false,
    "smile_confidence": 0.0,
    "head_pose": "centro",
    "head_confidence": 0.0
  }
}
```

## Instruções de Prova de Vida

1. **Piscar**: Detecta olhos fechados usando EAR (Eye Aspect Ratio) < 0.25
2. **Sorrir**: Detecta sorriso através da distância entre marcadores dos lábios
3. **Virar Esquerda**: Detecta rotação da cabeça para esquerda (yaw < -15°)
4. **Virar Direita**: Detecta rotação da cabeça para direita (yaw > 15°)

## Modelos Suportados

- VGG-Face (padrão)
- Facenet
- OpenFace
- DeepFace
- DeepID
- ArcFace
- Dlib

## Exemplo de Uso

### Python
```python
import requests

# Verificar faces
files = {
    'img1': open('foto1.jpg', 'rb'),
    'img2': open('foto2.jpg', 'rb')
}
response = requests.post('http://localhost:8000/api/deepface/verify', files=files)
print(response.json())
```

### cURL
```bash
# Health check
curl http://localhost:8000/api/deepface/health

# Verificar faces
curl -X POST http://localhost:8000/api/deepface/verify \
  -F "img1=@foto1.jpg" \
  -F "img2=@foto2.jpg"
```

## Notas Importantes

- Primeira execução: O DeepFace baixa modelos automaticamente (pode demorar)
- Requisitos de imagem: Mínimo 64x64 pixels, formato JPEG/PNG
- Performance: Processamento pode levar 1-5 segundos dependendo do modelo
- Memória: Requer pelo menos 2GB RAM disponível

## Troubleshooting

**Erro: "No module named 'tensorflow'"**
- Instale TensorFlow: `pip install tensorflow`

**Erro: "Face could not be detected"**
- Verifique qualidade e iluminação da imagem
- Tente com `enforce_detection=False` (não recomendado para produção)

**Erro: "CUDA out of memory"**
- Reduza o tamanho das imagens
- Use CPU ao invés de GPU

## Licença

Este serviço é parte do Sistema de Assinatura Digital.
