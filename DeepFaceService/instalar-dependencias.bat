@echo off
echo ========================================
echo   DeepFace Service - Instalando Dependencias
echo ========================================
echo.

REM Verificar se o Python está instalado
python --version >nul 2>&1
if errorlevel 1 (
    echo ERRO: Python nao encontrado!
    echo Por favor, instale o Python 3.8 ou superior.
    pause
    exit /b 1
)

REM Criar ambiente virtual se não existir
if not exist "venv\" (
    echo Criando ambiente virtual...
    python -m venv venv
    if errorlevel 1 (
        echo ERRO: Falha ao criar ambiente virtual!
        pause
        exit /b 1
    )
    echo Ambiente virtual criado com sucesso!
    echo.
)

REM Ativar ambiente virtual
echo Ativando ambiente virtual...
call venv\Scripts\activate.bat
if errorlevel 1 (
    echo ERRO: Falha ao ativar ambiente virtual!
    pause
    exit /b 1
)

REM Atualizar pip
echo Atualizando pip...
python -m pip install --upgrade pip

REM Atualizar setuptools e wheel (importante para Pillow)
echo Atualizando setuptools e wheel...
pip install --upgrade setuptools wheel

REM Instalar dependências
echo.
echo Instalando dependencias...
echo Isso pode demorar alguns minutos na primeira vez...
echo.

pip install -r requirements.txt

if errorlevel 1 (
    echo.
    echo ERRO: Falha ao instalar dependencias com requirements.txt!
    echo Tentando com versao alternativa...
    echo.
    pip install -r requirements-alternativo.txt
    if errorlevel 1 (
        echo.
        echo ERRO: Falha ao instalar dependencias!
        echo.
        echo Tente instalar manualmente:
        echo   pip install Pillow opencv-python numpy fastapi uvicorn deepface mediapipe tensorflow tf-keras
        pause
        exit /b 1
    )
)

echo.
echo ========================================
echo   Dependencias instaladas com sucesso!
echo ========================================
echo.
echo Agora voce pode executar:
echo   - iniciar-servico.bat (modo simples)
echo   - iniciar-servico-reload.bat (modo desenvolvimento)
echo.
pause
