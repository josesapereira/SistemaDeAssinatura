@echo off
echo ========================================
echo   Corrigindo MediaPipe
echo ========================================
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
echo Desinstalando MediaPipe antigo...
pip uninstall mediapipe -y

echo.
echo Instalando MediaPipe versao compativel (0.10.8)...
pip install mediapipe==0.10.8

if errorlevel 1 (
    echo.
    echo Tentando versao alternativa (0.10.7)...
    pip install mediapipe==0.10.7
)

echo.
echo Verificando instalacao...
python -c "import mediapipe; print('MediaPipe instalado:', mediapipe.__version__)"

echo.
echo Testando importacao...
python -c "import mediapipe as mp; print('MediaPipe import OK')"

echo.
echo Executando teste completo do MediaPipe...
python C:\projeto\SistemaDeAssinatura\DeepFaceService\testar-mediapipe.py

if errorlevel 1 (
    echo.
    echo AVISO: MediaPipe pode ter problemas de importacao.
    echo Verifique a saida do teste acima.
)

echo.
echo ========================================
echo   MediaPipe corrigido com sucesso!
echo ========================================
echo.
echo Agora voce pode executar: iniciar-servico.bat
echo.
pause
