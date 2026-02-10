"""
Módulo de Prova de Vida (Liveness Detection) usando MediaPipe
Detecta ações faciais em fotos: piscar, sorrir, virar cabeça
"""

import cv2
import numpy as np
from typing import Dict, Optional, Tuple
import random
import time
from datetime import datetime

# Importação do MediaPipe com tratamento de erro
MEDIAPIPE_AVAILABLE = False
mp = None
FaceLandmarker = None
mp_image = None
BaseOptions = None
RunningMode = None

try:
    import mediapipe as mp
    # MediaPipe 0.10.32+ usa tasks.vision em vez de solutions
    if hasattr(mp, 'tasks') and hasattr(mp.tasks, 'vision'):
        FaceLandmarker = mp.tasks.vision.FaceLandmarker
        BaseOptions = mp.tasks.BaseOptions
        RunningMode = mp.tasks.vision.RunningMode
        # Para processar imagens
        mp_image = mp.Image
        MEDIAPIPE_AVAILABLE = True
    # Fallback para versões antigas (se ainda existirem)
    elif hasattr(mp, 'solutions'):
        from mediapipe.python.solutions import face_mesh
        FaceLandmarker = face_mesh.FaceMesh  # Compatibilidade com código antigo
        MEDIAPIPE_AVAILABLE = True
except ImportError:
    pass

class LivenessDetector:
    """Detector de prova de vida usando MediaPipe Face Landmarker"""
    
    def __init__(self):
        if not MEDIAPIPE_AVAILABLE or FaceLandmarker is None:
            raise ImportError(
                "MediaPipe não está disponível ou não foi possível importar FaceLandmarker. "
                "Instale com: pip install 'mediapipe>=0.10.0,<0.11.0'"
            )
        
        try:
            # MediaPipe 0.10.32+ usa FaceLandmarker com BaseOptions
            if hasattr(mp, 'tasks') and hasattr(mp.tasks, 'vision'):
                import os
                
                # Caminho do modelo (deve estar no mesmo diretório do script)
                script_dir = os.path.dirname(os.path.abspath(__file__))
                model_path = os.path.join(script_dir, "face_landmarker.task")
                
                # Verificar se o modelo existe
                if not os.path.exists(model_path):
                    raise ImportError(
                        f"Modelo do MediaPipe não encontrado em: {model_path}\n"
                        f"Execute o script 'baixar-modelo-mediapipe.py' para baixar o modelo."
                    )
                
                # Criar opções com o modelo
                base_options = BaseOptions(model_asset_path=model_path)
                options = mp.tasks.vision.FaceLandmarkerOptions(
                    base_options=base_options,
                    output_face_blendshapes=False,
                    running_mode=RunningMode.IMAGE,
                    num_faces=1,
                    min_face_detection_confidence=0.5,
                    min_face_presence_confidence=0.5,
                    min_tracking_confidence=0.5
                )
                self.face_landmarker = FaceLandmarker.create_from_options(options)
                self.use_new_api = True
            else:
                # Fallback para API antiga (se disponível)
                self.face_landmarker = FaceLandmarker(
                    static_image_mode=True,
                    max_num_faces=1,
                    refine_landmarks=True,
                    min_detection_confidence=0.5,
                    min_tracking_confidence=0.5
                )
                self.use_new_api = False
        except Exception as e:
            raise ImportError(f"Erro ao inicializar MediaPipe Face Landmarker: {str(e)}")
        
        # Índices dos landmarks do MediaPipe Face Mesh
        # Olhos
        self.LEFT_EYE_INDICES = [33, 7, 163, 144, 145, 153, 154, 155, 133, 173, 157, 158, 159, 160, 161, 246]
        self.RIGHT_EYE_INDICES = [362, 382, 381, 380, 374, 373, 390, 249, 263, 466, 388, 387, 386, 385, 384, 398]
        
        # Pontos específicos para EAR (Eye Aspect Ratio)
        self.LEFT_EYE_POINTS = [33, 160, 158, 133, 153, 144]
        self.RIGHT_EYE_POINTS = [362, 385, 387, 263, 373, 380]
        
        # Lábios para detecção de sorriso
        self.UPPER_LIP_INDICES = [61, 84, 17, 314, 405, 320, 307, 375, 321, 308, 324, 318]
        self.LOWER_LIP_INDICES = [78, 95, 88, 178, 87, 14, 317, 402, 318, 324, 308, 415]
        
        # Pontos para estimativa de pose da cabeça
        self.FACE_OVAL_INDICES = [10, 338, 297, 332, 284, 251, 389, 356, 454, 323, 361, 288, 397, 365, 379, 378, 400, 377, 152, 148, 176, 149, 150, 136, 172, 58, 132, 93, 234, 127, 162, 21, 54, 103, 67, 109]
    
    def calculate_ear(self, landmarks, eye_indices: list) -> float:
        """Calcula EAR (Eye Aspect Ratio) para detectar olhos fechados"""
        if len(landmarks) < max(eye_indices):
            return 1.0
        
        # Extrair coordenadas dos pontos do olho
        eye_points = []
        for idx in eye_indices:
            if idx < len(landmarks):
                landmark = landmarks[idx]
                # Suportar tanto a API antiga (landmark.x) quanto a nova (landmark.x se for NormalizedLandmark)
                x = landmark.x if hasattr(landmark, 'x') else landmark[0] if isinstance(landmark, (list, tuple)) else 0
                y = landmark.y if hasattr(landmark, 'y') else landmark[1] if isinstance(landmark, (list, tuple)) else 0
                eye_points.append([x, y])
        
        if len(eye_points) < 6:
            return 1.0
        
        eye_points = np.array(eye_points)
        
        # Calcular distâncias verticais
        vertical_1 = np.linalg.norm(eye_points[1] - eye_points[5])
        vertical_2 = np.linalg.norm(eye_points[2] - eye_points[4])
        
        # Calcular distância horizontal
        horizontal = np.linalg.norm(eye_points[0] - eye_points[3])
        
        # Calcular EAR
        if horizontal == 0:
            return 1.0
        
        ear = (vertical_1 + vertical_2) / (2.0 * horizontal)
        return ear
    
    def detect_blink(self, landmarks) -> Tuple[bool, float]:
        """Detecta se os olhos estão fechados (piscando)"""
        left_ear = self.calculate_ear(landmarks, self.LEFT_EYE_POINTS)
        right_ear = self.calculate_ear(landmarks, self.RIGHT_EYE_POINTS)
        
        # EAR médio
        avg_ear = (left_ear + right_ear) / 2.0
        
        # EAR < 0.25 indica olhos fechados
        is_blinking = avg_ear < 0.25
        confidence = 1.0 - min(avg_ear / 0.25, 1.0) if is_blinking else 0.0
        
        return is_blinking, confidence
    
    def detect_smile(self, landmarks) -> Tuple[bool, float]:
        """Detecta se a pessoa está sorrindo"""
        if len(landmarks) < max(self.UPPER_LIP_INDICES + self.LOWER_LIP_INDICES):
            return False, 0.0
        
        # Extrair coordenadas dos lábios
        upper_lip_points = []
        lower_lip_points = []
        
        for idx in self.UPPER_LIP_INDICES:
            if idx < len(landmarks):
                landmark = landmarks[idx]
                x = landmark.x if hasattr(landmark, 'x') else landmark[0] if isinstance(landmark, (list, tuple)) else 0
                y = landmark.y if hasattr(landmark, 'y') else landmark[1] if isinstance(landmark, (list, tuple)) else 0
                upper_lip_points.append([x, y])
        
        for idx in self.LOWER_LIP_INDICES:
            if idx < len(landmarks):
                landmark = landmarks[idx]
                x = landmark.x if hasattr(landmark, 'x') else landmark[0] if isinstance(landmark, (list, tuple)) else 0
                y = landmark.y if hasattr(landmark, 'y') else landmark[1] if isinstance(landmark, (list, tuple)) else 0
                lower_lip_points.append([x, y])
        
        if len(upper_lip_points) < 6 or len(lower_lip_points) < 6:
            return False, 0.0
        
        upper_lip_points = np.array(upper_lip_points)
        lower_lip_points = np.array(lower_lip_points)
        
        # Calcular distância média entre lábios superior e inferior
        distances = []
        for upper_point in upper_lip_points[:6]:  # Usar primeiros 6 pontos
            for lower_point in lower_lip_points[:6]:
                dist = np.linalg.norm(upper_point - lower_point)
                distances.append(dist)
        
        avg_distance = np.mean(distances) if distances else 0.0
        
        # Distância maior indica sorriso (lábios mais separados)
        # Threshold ajustável baseado em testes
        is_smiling = avg_distance > 0.015  # Ajustar conforme necessário
        confidence = min(avg_distance / 0.02, 1.0) if is_smiling else 0.0
        
        return is_smiling, confidence
    
    def detect_head_pose(self, landmarks) -> Tuple[str, float]:
        """Detecta a rotação da cabeça (esquerda, direita, centro)"""
        if len(landmarks) < 10:
            return "centro", 0.0
        
        # Usar pontos do nariz e das bochechas para estimar rotação
        # Ponto do nariz (30)
        nose_tip_idx = 4  # Índice aproximado do nariz
        if nose_tip_idx >= len(landmarks):
            return "centro", 0.0
        
        nose_landmark = landmarks[nose_tip_idx]
        nose_tip = type('obj', (object,), {
            'x': nose_landmark.x if hasattr(nose_landmark, 'x') else nose_landmark[0] if isinstance(nose_landmark, (list, tuple)) else 0,
            'y': nose_landmark.y if hasattr(nose_landmark, 'y') else nose_landmark[1] if isinstance(nose_landmark, (list, tuple)) else 0
        })()
        
        # Pontos das bochechas (esquerda e direita)
        left_cheek_idx = 234  # Aproximado
        right_cheek_idx = 454  # Aproximado
        
        if left_cheek_idx >= len(landmarks) or right_cheek_idx >= len(landmarks):
            return "centro", 0.0
        
        left_cheek_landmark = landmarks[left_cheek_idx]
        right_cheek_landmark = landmarks[right_cheek_idx]
        left_cheek = type('obj', (object,), {
            'x': left_cheek_landmark.x if hasattr(left_cheek_landmark, 'x') else left_cheek_landmark[0] if isinstance(left_cheek_landmark, (list, tuple)) else 0,
            'y': left_cheek_landmark.y if hasattr(left_cheek_landmark, 'y') else left_cheek_landmark[1] if isinstance(left_cheek_landmark, (list, tuple)) else 0
        })()
        right_cheek = type('obj', (object,), {
            'x': right_cheek_landmark.x if hasattr(right_cheek_landmark, 'x') else right_cheek_landmark[0] if isinstance(right_cheek_landmark, (list, tuple)) else 0,
            'y': right_cheek_landmark.y if hasattr(right_cheek_landmark, 'y') else right_cheek_landmark[1] if isinstance(right_cheek_landmark, (list, tuple)) else 0
        })()
        
        # Calcular posição relativa do nariz
        # Se nariz está mais próximo da bochecha direita, cabeça virou esquerda
        # Se nariz está mais próximo da bochecha esquerda, cabeça virou direita
        
        dist_to_left = abs(nose_tip.x - left_cheek.x)
        dist_to_right = abs(nose_tip.x - right_cheek.x)
        
        # Calcular yaw angle aproximado
        center_x = (left_cheek.x + right_cheek.x) / 2.0
        yaw_angle = (nose_tip.x - center_x) * 100  # Normalizar
        
        if yaw_angle < -15:
            return "esquerda", min(abs(yaw_angle) / 45.0, 1.0)
        elif yaw_angle > 15:
            return "direita", min(abs(yaw_angle) / 45.0, 1.0)
        else:
            return "centro", 0.0
    
    def validate_action(self, image: np.ndarray, instruction: str) -> Dict:
        """
        Valida se a ação solicitada está presente na foto
        
        Args:
            image: Imagem numpy array (RGB)
            instruction: Ação esperada ("piscar", "sorrir", "virar_esquerda", "virar_direita")
        
        Returns:
            Dict com resultado da validação
        """
        try:
            # MediaPipe espera RGB
            # Converter BGR para RGB se necessário (OpenCV usa BGR por padrão)
            if len(image.shape) == 3:
                # Tentar detectar se é BGR (OpenCV) ou RGB (PIL)
                # Por padrão, assumir que imagens numpy vêm em RGB do PIL
                # Se vier do OpenCV diretamente, pode estar em BGR
                rgb_image = image.copy()
                # MediaPipe processa RGB nativamente
            else:
                rgb_image = image
            
            # Processar com MediaPipe
            if self.use_new_api:
                # Nova API (0.10.32+): converter numpy array para mp.Image
                height, width = rgb_image.shape[:2]
                mp_image_obj = mp_image(image_format=mp.ImageFormat.SRGB, data=rgb_image)
                results = self.face_landmarker.detect(mp_image_obj)
                
                if not results.face_landmarks or len(results.face_landmarks) == 0:
                    return {
                        "aprovado": False,
                        "acao_detectada": "nenhuma",
                        "confianca": 0.0,
                        "detalhes": {
                            "erro": "Nenhuma face detectada na imagem"
                        }
                    }
                
                # Pegar primeira face detectada
                # Na nova API, face_landmarks é uma lista de NormalizedLandmark
                landmarks = results.face_landmarks[0]
            else:
                # API antiga
                results = self.face_landmarker.process(rgb_image)
                
                if not results.multi_face_landmarks:
                    return {
                        "aprovado": False,
                        "acao_detectada": "nenhuma",
                        "confianca": 0.0,
                        "detalhes": {
                            "erro": "Nenhuma face detectada na imagem"
                        }
                    }
                
                # Pegar primeira face detectada
                face_landmarks = results.multi_face_landmarks[0]
                landmarks = face_landmarks.landmark
            
            # Detectar ações
            is_blinking, blink_confidence = self.detect_blink(landmarks)
            is_smiling, smile_confidence = self.detect_smile(landmarks)
            head_pose, head_confidence = self.detect_head_pose(landmarks)
            
            # Mapear instrução para ação detectada
            action_detected = None
            confidence = 0.0
            
            if instruction == "piscar":
                action_detected = "piscar" if is_blinking else "olhos_abertos"
                confidence = blink_confidence
            
            elif instruction == "sorrir":
                action_detected = "sorrir" if is_smiling else "sem_sorriso"
                confidence = smile_confidence
            
            elif instruction == "virar_esquerda":
                action_detected = "esquerda" if head_pose == "esquerda" else head_pose
                confidence = head_confidence if head_pose == "esquerda" else 0.0
            
            elif instruction == "virar_direita":
                action_detected = "direita" if head_pose == "direita" else head_pose
                confidence = head_confidence if head_pose == "direita" else 0.0
            
            # Validar se ação corresponde à instrução
            approved = False
            if instruction == "piscar" and is_blinking:
                approved = confidence > 0.7
            elif instruction == "sorrir" and is_smiling:
                approved = confidence > 0.7
            elif instruction == "virar_esquerda" and head_pose == "esquerda":
                approved = confidence > 0.7
            elif instruction == "virar_direita" and head_pose == "direita":
                approved = confidence > 0.7
            
            return {
                "aprovado": approved,
                "acao_detectada": action_detected,
                "confianca": round(confidence, 3),
                "detalhes": {
                    "instrucao_solicitada": instruction,
                    "blink_detectado": is_blinking,
                    "blink_confidence": round(blink_confidence, 3),
                    "smile_detectado": is_smiling,
                    "smile_confidence": round(smile_confidence, 3),
                    "head_pose": head_pose,
                    "head_confidence": round(head_confidence, 3)
                }
            }
        
        except Exception as e:
            return {
                "aprovado": False,
                "acao_detectada": "erro",
                "confianca": 0.0,
                "detalhes": {
                    "erro": str(e)
                }
            }

# ==================== Geração de Instruções ====================

_LAST_INSTRUCTION = None

def generate_random_instruction(last_instruction: Optional[str] = None) -> Dict:
    """
    Gera uma instrução aleatória de prova de vida
    
    Args:
        last_instruction: Última instrução gerada (para evitar repetição)
    
    Returns:
        Dict com instrução gerada
    """
    global _LAST_INSTRUCTION
    
    instructions = {
        "piscar": "Por favor, pisque os olhos agora",
        "sorrir": "Por favor, sorria para a câmera",
        "virar_esquerda": "Por favor, vire a cabeça para a esquerda",
        "virar_direita": "Por favor, vire a cabeça para a direita"
    }
    
    # Remover última instrução se fornecida
    available_actions = list(instructions.keys())
    if last_instruction and last_instruction in available_actions:
        available_actions.remove(last_instruction)
    elif _LAST_INSTRUCTION and _LAST_INSTRUCTION in available_actions:
        available_actions.remove(_LAST_INSTRUCTION)
    
    # Selecionar ação aleatória
    selected_action = random.choice(available_actions)
    _LAST_INSTRUCTION = selected_action
    
    return {
        "acao": selected_action,
        "texto": instructions[selected_action],
        "timestamp": datetime.now().isoformat()
    }
