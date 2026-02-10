from fastapi import FastAPI, File, UploadFile, HTTPException, Form
from starlette.requests import Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from pydantic import BaseModel
from typing import Optional, List
import base64
import io
from PIL import Image
import numpy as np
from deepface import DeepFace
import cv2
# Tentar importar módulo de prova de vida (opcional)
liveness_available = False
LivenessDetector = None
generate_random_instruction = None
liveness_detector = None

try:
    from liveness import LivenessDetector, generate_random_instruction
    # Tentar inicializar apenas se a importação foi bem-sucedida
    try:
        liveness_detector = LivenessDetector()
        liveness_available = True
        print("✓ Módulo de prova de vida (MediaPipe) carregado com sucesso")
    except ImportError as e:
        print(f"⚠ AVISO: MediaPipe não disponível ou não foi possível inicializar")
        print(f"⚠ Erro: {str(e)}")
        print("⚠ Endpoints de prova de vida estarão desabilitados")
        print("⚠ O serviço continuará funcionando para verificação, análise e detecção de faces")
        liveness_available = False
        liveness_detector = None
    except Exception as e:
        print(f"⚠ AVISO: Erro ao inicializar MediaPipe: {str(e)}")
        print("⚠ Endpoints de prova de vida estarão desabilitados")
        print("⚠ O serviço continuará funcionando para verificação, análise e detecção de faces")
        liveness_available = False
        liveness_detector = None
except ImportError as e:
    print(f"⚠ AVISO: Módulo de prova de vida não disponível: {str(e)}")
    print("⚠ Endpoints de prova de vida estarão desabilitados")
    print("⚠ O serviço continuará funcionando para verificação, análise e detecção de faces")
    print("⚠ Para habilitar prova de vida, instale: pip install 'mediapipe>=0.10.0,<0.11.0'")
    liveness_available = False
except Exception as e:
    print(f"⚠ AVISO: Erro ao importar módulo de prova de vida: {str(e)}")
    print("⚠ Endpoints de prova de vida estarão desabilitados")
    print("⚠ O serviço continuará funcionando para verificação, análise e detecção de faces")
    liveness_available = False

app = FastAPI(title="DeepFace Service", version="1.0.0")

# Configurar CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Modelos suportados pelo DeepFace
SUPPORTED_MODELS = ["VGG-Face", "Facenet", "OpenFace", "DeepFace", "DeepID", "ArcFace", "Dlib"]

# ==================== DTOs ====================

class VerifyRequest(BaseModel):
    img1_base64: Optional[str] = None
    img2_base64: Optional[str] = None
    model_name: str = "VGG-Face"
    distance_metric: str = "cosine"

class AnalyzeRequest(BaseModel):
    img_base64: Optional[str] = None
    actions: Optional[List[str]] = None  # ["emotion", "age", "gender", "race"]

class DetectRequest(BaseModel):
    img_base64: Optional[str] = None

class LivenessValidationRequest(BaseModel):
    img_base64: Optional[str] = None
    instruction: str  # "piscar", "sorrir", "virar_esquerda", "virar_direita"

# ==================== Utilitários ====================

def base64_to_image(base64_string: str) -> np.ndarray:
    """Converte string base64 para imagem numpy array"""
    try:
        # Remove prefixo se existir (data:image/jpeg;base64,...)
        if "," in base64_string:
            base64_string = base64_string.split(",")[1]
        
        image_data = base64.b64decode(base64_string)
        image = Image.open(io.BytesIO(image_data))
        # Converte para RGB se necessário
        if image.mode != "RGB":
            image = image.convert("RGB")
        return np.array(image)
    except Exception as e:
        raise HTTPException(status_code=400, detail=f"Erro ao decodificar imagem base64: {str(e)}")

def image_to_base64(image: np.ndarray) -> str:
    """Converte imagem numpy array para base64"""
    try:
        pil_image = Image.fromarray(image)
        buffer = io.BytesIO()
        pil_image.save(buffer, format="JPEG")
        return base64.b64encode(buffer.getvalue()).decode("utf-8")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao codificar imagem: {str(e)}")

def file_to_image(file: UploadFile) -> np.ndarray:
    """Converte arquivo UploadFile para imagem numpy array"""
    try:
        contents = file.file.read()
        image = Image.open(io.BytesIO(contents))
        if image.mode != "RGB":
            image = image.convert("RGB")
        return np.array(image)
    except Exception as e:
        raise HTTPException(status_code=400, detail=f"Erro ao processar arquivo: {str(e)}")
    finally:
        file.file.seek(0)

# ==================== Health Check ====================

@app.get("/api/deepface/health")
async def health_check():
    """Health check do serviço"""
    return {
        "status": "healthy",
        "service": "DeepFace Service",
        "version": "1.0.0",
        "liveness_available": liveness_available,
        "features": {
            "verify": True,
            "analyze": True,
            "detect": True,
            "liveness": liveness_available
        }
    }

# ==================== Verificação de Identidade ====================

@app.post("/api/deepface/verify")
async def verify_faces(
    img1: Optional[UploadFile] = File(None),
    img2: Optional[UploadFile] = File(None),
    img1_base64: Optional[str] = Form(None),
    img2_base64: Optional[str] = Form(None),
    model_name: str = Form("VGG-Face"),
    distance_metric: str = Form("cosine")
):
    """
    Verifica se duas faces pertencem à mesma pessoa
    
    Aceita imagens via multipart/form-data (img1, img2) ou base64 (img1_base64, img2_base64)
    """
    try:
        # Validar modelo
        if model_name not in SUPPORTED_MODELS:
            raise HTTPException(
                status_code=400,
                detail=f"Modelo não suportado. Modelos disponíveis: {', '.join(SUPPORTED_MODELS)}"
            )
        
        # Obter imagens
        if img1 and img2:
            img1_array = file_to_image(img1)
            img2_array = file_to_image(img2)
        elif img1_base64 and img2_base64:
            img1_array = base64_to_image(img1_base64)
            img2_array = base64_to_image(img2_base64)
        else:
            raise HTTPException(
                status_code=400,
                detail="É necessário fornecer duas imagens (img1/img2 ou img1_base64/img2_base64)"
            )
        
        # Verificar faces
        result = DeepFace.verify(
            img1_path=img1_array,
            img2_path=img2_array,
            model_name=model_name,
            distance_metric=distance_metric,
            enforce_detection=True
        )
        
        return {
            "verificado": result["verified"],
            "distancia": result["distance"],
            "limiar": result["threshold"],
            "modelo": model_name,
            "metrica": distance_metric
        }
    
    except ValueError as e:
        raise HTTPException(status_code=400, detail=f"Erro na verificação: {str(e)}")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro interno: {str(e)}")

# ==================== Análise de Atributos ====================

@app.post("/api/deepface/analyze")
async def analyze_face(
    img: Optional[UploadFile] = File(None),
    img_base64: Optional[str] = Form(None),
    actions: Optional[str] = Form(None)  # "emotion,age,gender,race"
):
    """
    Analisa atributos faciais: emoções, idade, gênero, raça
    
    Aceita imagem via multipart/form-data (img) ou base64 (img_base64)
    actions: lista separada por vírgula (ex: "emotion,age,gender,race")
    """
    try:
        # Obter imagem
        if img:
            img_array = file_to_image(img)
        elif img_base64:
            img_array = base64_to_image(img_base64)
        else:
            raise HTTPException(
                status_code=400,
                detail="É necessário fornecer uma imagem (img ou img_base64)"
            )
        
        # Processar actions
        actions_list = None
        if actions:
            actions_list = [a.strip() for a in actions.split(",")]
            valid_actions = ["emotion", "age", "gender", "race"]
            invalid_actions = [a for a in actions_list if a not in valid_actions]
            if invalid_actions:
                raise HTTPException(
                    status_code=400,
                    detail=f"Ações inválidas: {', '.join(invalid_actions)}. Ações válidas: {', '.join(valid_actions)}"
                )
        
        # Analisar face
        result = DeepFace.analyze(
            img_path=img_array,
            actions=actions_list,
            enforce_detection=True
        )
        
        # Se resultado é lista, pegar primeiro elemento
        if isinstance(result, list):
            result = result[0]
        
        return {
            "emotion": result.get("dominant_emotion"),
            "emotion_scores": result.get("emotion"),
            "age": result.get("age"),
            "gender": result.get("dominant_gender"),
            "gender_scores": {
                "Man": result.get("gender", {}).get("Man", 0),
                "Woman": result.get("gender", {}).get("Woman", 0)
            },
            "race": result.get("dominant_race"),
            "race_scores": result.get("race")
        }
    
    except ValueError as e:
        raise HTTPException(status_code=400, detail=f"Erro na análise: {str(e)}")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro interno: {str(e)}")

# ==================== Detecção de Faces ====================

@app.post("/api/deepface/detect")
async def detect_faces(
    img: Optional[UploadFile] = File(None),
    img_base64: Optional[str] = Form(None)
):
    """
    Detecta faces em uma imagem
    
    Aceita imagem via multipart/form-data (img) ou base64 (img_base64)
    """
    try:
        # Obter imagem
        if img:
            img_array = file_to_image(img)
        elif img_base64:
            img_array = base64_to_image(img_base64)
        else:
            raise HTTPException(
                status_code=400,
                detail="É necessário fornecer uma imagem (img ou img_base64)"
            )
        
        # Detectar faces
        result = DeepFace.extract_faces(
            img_path=img_array,
            enforce_detection=False
        )
        
        faces_detected = []
        for i, face in enumerate(result):
            faces_detected.append({
                "face_index": i,
                "region": face.get("facial_area", {}),
                "confidence": face.get("confidence", 1.0)
            })
        
        return {
            "faces_detectadas": len(faces_detected),
            "faces": faces_detected
        }
    
    except ValueError as e:
        raise HTTPException(status_code=400, detail=f"Erro na detecção: {str(e)}")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro interno: {str(e)}")

# ==================== Prova de Vida - Gerar Instrução ====================

@app.post("/api/deepface/liveness/generate-instruction")
async def generate_liveness_instruction(
    last_instruction: Optional[str] = Form(default=None)
):
    """
    Gera uma instrução aleatória de prova de vida
    
    Instruções possíveis:
    - piscar: "Por favor, pisque os olhos agora"
    - sorrir: "Por favor, sorria para a câmera"
    - virar_esquerda: "Por favor, vire a cabeça para a esquerda"
    - virar_direita: "Por favor, vire a cabeça para a direita"
    """
    if not liveness_available or generate_random_instruction is None:
        raise HTTPException(
            status_code=503, 
            detail="Módulo de prova de vida não disponível. Verifique a instalação do MediaPipe."
        )
    
    try:
        # Tratar string vazia, None ou valores vazios como None
        last_instruction_value = None
        if last_instruction:
            stripped = last_instruction.strip()
            if stripped:
                last_instruction_value = stripped
        
        instruction_data = generate_random_instruction(last_instruction_value)
        
        return {
            "instrucao": instruction_data["texto"],
            "acao": instruction_data["acao"],
            "timestamp": instruction_data["timestamp"]
        }
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao gerar instrução: {str(e)}")

# ==================== Prova de Vida - Validar ====================

@app.post("/api/deepface/liveness/validate")
async def validate_liveness(
    img: Optional[UploadFile] = File(None),
    img_base64: Optional[str] = Form(None),
    instruction: str = Form(...)
):
    """
    Valida prova de vida em uma foto
    
    Aceita imagem via multipart/form-data (img) ou base64 (img_base64)
    instruction: ação esperada ("piscar", "sorrir", "virar_esquerda", "virar_direita")
    """
    if not liveness_available or liveness_detector is None:
        raise HTTPException(
            status_code=503, 
            detail="Módulo de prova de vida não disponível. Verifique a instalação do MediaPipe."
        )
    
    try:
        # Validar instrução
        valid_instructions = ["piscar", "sorrir", "virar_esquerda", "virar_direita"]
        if instruction not in valid_instructions:
            raise HTTPException(
                status_code=400,
                detail=f"Instrução inválida. Instruções válidas: {', '.join(valid_instructions)}"
            )
        
        # Obter imagem
        if img:
            img_array = file_to_image(img)
        elif img_base64:
            img_array = base64_to_image(img_base64)
        else:
            raise HTTPException(
                status_code=400,
                detail="É necessário fornecer uma imagem (img ou img_base64)"
            )
        
        # Validar prova de vida
        result = liveness_detector.validate_action(img_array, instruction)
        
        return {
            "aprovado": result["aprovado"],
            "acao_detectada": result["acao_detectada"],
            "confianca": result["confianca"],
            "detalhes": result.get("detalhes", {})
        }
    
    except ValueError as e:
        raise HTTPException(status_code=400, detail=f"Erro na validação: {str(e)}")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro interno: {str(e)}")

# ==================== Prova de Vida - Validar Múltiplas Fotos ====================

@app.post("/api/deepface/liveness/validate-multiple")
async def validate_liveness_multiple(
    request: Request
):
    """
    Valida prova de vida em múltiplas fotos com instruções aleatórias
    
    Aceita múltiplas fotos via multipart/form-data no formato:
    fotos[0].img_base64, fotos[0].instruction, fotos[1].img_base64, fotos[1].instruction, etc.
    """
    if not liveness_available or liveness_detector is None:
        raise HTTPException(
            status_code=503, 
            detail="Módulo de prova de vida não disponível. Verifique a instalação do MediaPipe."
        )
    
    try:
        # Obter dados do formulário
        form_data = await request.form()
        
        # Obter total de fotos (se fornecido)
        total_fotos = None
        if "total_fotos" in form_data:
            try:
                total_fotos = int(form_data["total_fotos"])
            except (ValueError, TypeError):
                pass
        
        # Extrair fotos e instruções
        fotos = []
        i = 0
        
        # Se total_fotos foi fornecido, usar esse valor, senão tentar até não encontrar mais
        max_iterations = total_fotos if total_fotos else 100  # Limite de segurança
        
        while i < max_iterations:
            # Usar formato sem colchetes
            img_base64_key = f"fotos_{i}_img_base64"
            instruction_key = f"fotos_{i}_instruction"
            
            if img_base64_key not in form_data or instruction_key not in form_data:
                # Se total_fotos foi fornecido e já processamos todas, parar
                if total_fotos and i >= total_fotos:
                    break
                # Se não foi fornecido, tentar uma vez mais e depois parar
                if not total_fotos:
                    break
                i += 1
                continue
            
            img_base64 = form_data[img_base64_key]
            instruction = form_data[instruction_key]
            
            # Validar instrução
            valid_instructions = ["piscar", "sorrir", "virar_esquerda", "virar_direita"]
            if instruction not in valid_instructions:
                raise HTTPException(
                    status_code=400,
                    detail=f"Instrução inválida na foto {i+1}. Instruções válidas: {', '.join(valid_instructions)}"
                )
            
            # Converter base64 para imagem
            img_array = base64_to_image(img_base64)
            
            fotos.append({
                "imagem": img_array,
                "instrucao": instruction,
                "indice": i
            })
            
            i += 1
        
        if len(fotos) == 0:
            raise HTTPException(
                status_code=400,
                detail="É necessário fornecer pelo menos uma foto"
            )
        
        # Validar cada foto
        resultados = []
        fotos_aprovadas = 0
        
        for foto_data in fotos:
            result = liveness_detector.validate_action(
                foto_data["imagem"], 
                foto_data["instrucao"]
            )
            
            resultados.append({
                "aprovado": result["aprovado"],
                "acao_detectada": result["acao_detectada"],
                "confianca": result["confianca"],
                "detalhes": result.get("detalhes", {})
            })
            
            if result["aprovado"]:
                fotos_aprovadas += 1
        
        # Aprovar se pelo menos 70% das fotos foram aprovadas
        percentual_aprovado = (fotos_aprovadas / len(fotos)) * 100
        aprovado_geral = percentual_aprovado >= 70.0
        
        return {
            "aprovado": aprovado_geral,
            "total_fotos": len(fotos),
            "fotos_aprovadas": fotos_aprovadas,
            "resultados": resultados
        }
    
    except ValueError as e:
        raise HTTPException(status_code=400, detail=f"Erro na validação: {str(e)}")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro interno: {str(e)}")

# ==================== Main ====================

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8081)
