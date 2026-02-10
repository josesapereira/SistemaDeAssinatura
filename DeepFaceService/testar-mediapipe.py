#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
Script para testar a importação do MediaPipe
"""

print("Testando importação do MediaPipe...")
print("=" * 50)

try:
    import mediapipe as mp
    print(f"✓ MediaPipe importado: versão {mp.__version__ if hasattr(mp, '__version__') else 'desconhecida'}")
except ImportError as e:
    print(f"✗ Erro ao importar mediapipe: {e}")
    exit(1)

print("\nTestando diferentes formas de acesso ao face_mesh:")
print("-" * 50)

# Teste 1: mp.solutions.face_mesh
try:
    face_mesh = mp.solutions.face_mesh
    print("✓ mp.solutions.face_mesh: OK")
    print(f"  Tipo: {type(face_mesh)}")
except AttributeError as e:
    print(f"✗ mp.solutions.face_mesh: {e}")

# Teste 2: from mediapipe.python.solutions import face_mesh
try:
    from mediapipe.python.solutions import face_mesh
    print("✓ from mediapipe.python.solutions import face_mesh: OK")
    print(f"  Tipo: {type(face_mesh)}")
except ImportError as e:
    print(f"✗ from mediapipe.python.solutions import face_mesh: {e}")

# Teste 3: from mediapipe import solutions
try:
    from mediapipe import solutions
    print("✓ from mediapipe import solutions: OK")
    if hasattr(solutions, 'face_mesh'):
        print("  ✓ solutions.face_mesh disponível")
    else:
        print("  ✗ solutions.face_mesh NÃO disponível")
except ImportError as e:
    print(f"✗ from mediapipe import solutions: {e}")

# Teste 4: Verificar estrutura do mp
print("\nEstrutura do módulo mediapipe:")
print("-" * 50)
print(f"  Dir: {[x for x in dir(mp) if not x.startswith('_')][:10]}")

# Teste 5: Tentar criar FaceMesh
print("\nTestando criação de FaceMesh:")
print("-" * 50)
try:
    if hasattr(mp, 'solutions') and hasattr(mp.solutions, 'face_mesh'):
        fm = mp.solutions.face_mesh.FaceMesh(static_image_mode=True)
        print("✓ FaceMesh criado com sucesso via mp.solutions.face_mesh.FaceMesh")
        fm.close()
    else:
        print("✗ Não foi possível criar FaceMesh via mp.solutions.face_mesh")
except Exception as e:
    print(f"✗ Erro ao criar FaceMesh: {e}")

print("\n" + "=" * 50)
print("Teste concluído!")
