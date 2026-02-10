#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
Script para baixar o modelo padrão do MediaPipe FaceLandmarker
"""

import os
import urllib.request
import sys

# URL do modelo padrão do FaceLandmarker do MediaPipe
MODEL_URL = "https://storage.googleapis.com/mediapipe-models/face_landmarker/face_landmarker/float16/1/face_landmarker.task"

def download_model(output_path: str = "face_landmarker.task"):
    """Baixa o modelo do FaceLandmarker"""
    print(f"Baixando modelo do MediaPipe FaceLandmarker...")
    print(f"URL: {MODEL_URL}")
    print(f"Destino: {output_path}")
    
    try:
        urllib.request.urlretrieve(MODEL_URL, output_path)
        file_size = os.path.getsize(output_path)
        print(f"[OK] Modelo baixado com sucesso!")
        print(f"Tamanho: {file_size / (1024*1024):.2f} MB")
        return output_path
    except Exception as e:
        print(f"[ERRO] Falha ao baixar modelo: {e}")
        return None

if __name__ == "__main__":
    # Baixar para o diretório atual
    model_path = download_model()
    if model_path:
        print(f"\nModelo salvo em: {os.path.abspath(model_path)}")
        print(f"\nPara usar o modelo, atualize o codigo para:")
        print(f'base_options = BaseOptions(model_asset_path="{os.path.abspath(model_path)}")')
    else:
        sys.exit(1)
