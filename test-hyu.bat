@echo off
echo ========================================
echo Ejecutando Tests BlackBoxHYU
echo ========================================
echo.

cd /d C:\Users\deuga\Escritorio\MicroMercado\PruebasMicroMercado

echo Compilando proyecto de pruebas...
dotnet build --configuration Release --nologo -v quiet

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: La compilacion fallo
    pause
    exit /b 1
)

echo.
echo Ejecutando pruebas BlackBoxHYU...
echo.

dotnet test --filter "FullyQualifiedName~BlackBoxHYU" --logger "console;verbosity=normal" --nologo --no-build --configuration Release

echo.
echo ========================================
echo Pruebas completadas
echo ========================================
echo.
pause
