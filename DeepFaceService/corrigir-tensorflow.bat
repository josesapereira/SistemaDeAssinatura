@echo off
echo ========================================
echo   Corrigindo TensorFlow e tf-keras
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
echo Instalando tf-keras...
pip install tf-keras>=2.15.0

if errorlevel 1 (
    echo.
    echo ERRO: Falha ao instalar tf-keras!
    echo Tentando downgrade do TensorFlow...
    echo.
    pip install "tensorflow>=2.13.0,<2.20.0"
    pip install tf-keras>=2.15.0
)

echo.
echo ========================================
echo   Correcao concluida!
echo ========================================
echo.
echo Agora voce pode executar: iniciar-servico.bat
echo.
pause
