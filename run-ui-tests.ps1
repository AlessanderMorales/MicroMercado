# Script para ejecutar las pruebas UI con MicroMercado
# Asegúrate de tener MicroMercado corriendo antes de ejecutar este script

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Pruebas UI - MicroMercado" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que Chrome esté instalado
$chrome = Get-ItemProperty HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\App` Paths\chrome.exe -ErrorAction SilentlyContinue
if (-not $chrome) {
    Write-Host "[ERROR] Google Chrome no está instalado" -ForegroundColor Red
    Write-Host "Por favor, instala Chrome desde: https://www.google.com/chrome/" -ForegroundColor Yellow
    exit 1
}
Write-Host "[OK] Google Chrome encontrado" -ForegroundColor Green

# Verificar que MicroMercado esté corriendo
Write-Host ""
Write-Host "Verificando si MicroMercado está corriendo en https://localhost:7040..." -ForegroundColor Yellow
try {
    # Intentar conectar sin validar certificado SSL
    [Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
    $response = Invoke-WebRequest -Uri "https://localhost:7040" -UseBasicParsing -ErrorAction Stop -TimeoutSec 5
    Write-Host "[OK] MicroMercado está corriendo" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] MicroMercado NO está corriendo o no es accesible" -ForegroundColor Red
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host " INSTRUCCIONES PARA INICIAR MICROMERCADO" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Abre OTRA terminal (PowerShell o CMD)" -ForegroundColor White
    Write-Host "2. Navega al directorio de MicroMercado:" -ForegroundColor White
    Write-Host "   cd C:\Users\deuga\Escritorio\MicroMercado\MicroMercado" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "3. Ejecuta la aplicación:" -ForegroundColor White
    Write-Host "   dotnet run" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "4. Espera a ver este mensaje:" -ForegroundColor White
    Write-Host "   'Now listening on: https://localhost:7040'" -ForegroundColor Green
    Write-Host ""
    Write-Host "5. LUEGO vuelve a esta terminal y ejecuta este script de nuevo" -ForegroundColor White
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "¿Quieres continuar de todas formas? (y/n)"
    if ($continue -ne 'y' -and $continue -ne 'Y') {
        exit 1
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Ejecutando Pruebas..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANTE: Las pruebas tardarán aproximadamente 3-5 minutos" -ForegroundColor Yellow
Write-Host "Por favor, NO cierres MicroMercado mientras las pruebas se ejecutan" -ForegroundColor Yellow
Write-Host ""

# Ejecutar las pruebas
dotnet test PruebasUIBased/PruebasUIBased.csproj --logger "console;verbosity=normal"

$exitCode = $LASTEXITCODE

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Pruebas Completadas" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($exitCode -eq 0) {
    Write-Host ""
    Write-Host "? TODAS LAS PRUEBAS PASARON EXITOSAMENTE" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "?? ALGUNAS PRUEBAS FALLARON" -ForegroundColor Yellow
    Write-Host "Revisa los logs arriba para más detalles" -ForegroundColor Yellow
}

exit $exitCode
