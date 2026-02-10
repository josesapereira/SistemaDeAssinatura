@echo off
echo ========================================
echo   DeepFace Service - Iniciando Servico
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

REM Verificar se o ambiente virtual existe
if not exist "venv\" (
    echo Ambiente virtual nao encontrado. Criando...
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

REM Verificar se as dependências estão instaladas
echo Verificando dependencias...
pip show fastapi >nul 2>&1
if errorlevel 1 (
    echo Dependencias nao encontradas. Instalando...
    pip install -r requirements.txt
    if errorlevel 1 (
        echo ERRO: Falha ao instalar dependencias!
        pause
        exit /b 1
    )
    echo Dependencias instaladas com sucesso!
    echo.
)

REM Iniciar o servico
echo.
echo ========================================
echo   Iniciando servico na porta 8000...
echo ========================================
echo.
echo Pressione Ctrl+C para parar o servico
echo.

python app.py

REM Se o servico parar, manter a janela aberta
echo.
echo Servico encerrado.
pause
