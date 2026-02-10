@echo off
echo ========================================
echo   Instalando MediaPipe Versao Correta
echo ========================================
echo.
echo Este script instala MediaPipe 0.10.8 que suporta
echo a API solutions.face_mesh necessaria para o liveness.
echo.

REM Verificar se o ambiente virtual existe
if not exist "venv\" (
    echo ERRO: Ambiente virtual nao encontrado!
    echo Execute primeiro: instalar-dependencias.bat
    pause
    exit /b 1
)

REM Ativar ambiente virtual
echo Ativando ambiente virtual...
call venv\Scripts\activate.bat
if errorlevel 1 (
    echo ERRO: Falha ao ativar ambiente virtual!
    pause
    exit /b 1
)

echo.
echo Desinstalando MediaPipe atual (se existir)...
pip uninstall mediapipe -y >nul 2>&1

echo.
echo Instalando MediaPipe 0.10.8 (versao compativel)...
pip install mediapipe==0.10.8

if errorlevel 1 (
    echo.
    echo ERRO: Falha ao instalar MediaPipe 0.10.8
    echo Tentando versao alternativa 0.10.7...
    pip install mediapipe==0.10.7
    if errorlevel 1 (
        echo.
        echo ERRO: Falha ao instalar MediaPipe!
        echo Verifique sua conexao com a internet e tente novamente.
        pause
        exit /b 1
    )
)

echo.
echo Verificando instalacao...
python -c "import mediapipe as mp; print('MediaPipe instalado:', mp.__version__ if hasattr(mp, '__version__') else 'versao desconhecida')"

if errorlevel 1 (
    echo.
    echo AVISO: MediaPipe instalado mas nao foi possivel verificar versao.
)

echo.
echo Testando importacao e acesso ao face_mesh...
python -c "import mediapipe as mp; fm = mp.solutions.face_mesh.FaceMesh(static_image_mode=True); print('FaceMesh OK'); fm.close()"

if errorlevel 1 (
    echo.
    echo AVISO: MediaPipe instalado mas face_mesh nao esta acessivel.
    echo Execute: python testar-mediapipe-completo.py
    echo para diagnosticar o problema.
) else (
    echo.
    echo ========================================
    echo   MediaPipe instalado com sucesso!
    echo ========================================
)

echo.
pause
