# Script para iniciar MicroMercado y ejecutar las pruebas UI automáticamente
# Este script abre MicroMercado en una nueva ventana y luego ejecuta las pruebas

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Inicio Automático - Pruebas UI" -ForegroundColor Cyan
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

# Verificar que exista el proyecto MicroMercado
$microMercadoPath = "MicroMercado\MicroMercado.csproj"
if (-not (Test-Path $microMercadoPath)) {
    Write-Host "[ERROR] No se encuentra el proyecto MicroMercado" -ForegroundColor Red
    Write-Host "Ruta esperada: $microMercadoPath" -ForegroundColor Yellow
    exit 1
}
Write-Host "[OK] Proyecto MicroMercado encontrado" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host " Iniciando MicroMercado..." -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""

# Iniciar MicroMercado en una nueva ventana de PowerShell
Write-Host "Abriendo MicroMercado en una nueva ventana..." -ForegroundColor Cyan
$microMercadoProcess = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd MicroMercado; Write-Host 'Iniciando MicroMercado...' -ForegroundColor Green; dotnet run" -PassThru

Write-Host "Esperando 10 segundos para que MicroMercado inicie..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Verificar que MicroMercado esté corriendo
Write-Host ""
Write-Host "Verificando conexión a https://localhost:7040..." -ForegroundColor Yellow
$attempts = 0
$maxAttempts = 12  # 12 intentos x 5 segundos = 1 minuto
$connected = $false

while ($attempts -lt $maxAttempts -and -not $connected) {
    $attempts++
    Write-Host "Intento $attempts de $maxAttempts..." -ForegroundColor Gray
    
    try {
        [Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
        $response = Invoke-WebRequest -Uri "https://localhost:7040" -UseBasicParsing -ErrorAction Stop -TimeoutSec 5
        $connected = $true
        Write-Host "[OK] MicroMercado está corriendo y responde" -ForegroundColor Green
    } catch {
        if ($attempts -lt $maxAttempts) {
            Write-Host "Aún no responde, esperando 5 segundos más..." -ForegroundColor Gray
            Start-Sleep -Seconds 5
        }
    }
}

if (-not $connected) {
    Write-Host ""
    Write-Host "[ERROR] MicroMercado no respondió después de $maxAttempts intentos" -ForegroundColor Red
    Write-Host "Por favor, verifica la ventana de MicroMercado para ver si hay errores" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Presiona cualquier tecla para cerrar MicroMercado y salir..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    
    # Intentar cerrar el proceso de MicroMercado
    if ($microMercadoProcess -and -not $microMercadoProcess.HasExited) {
        Write-Host "Cerrando MicroMercado..." -ForegroundColor Yellow
        Stop-Process -Id $microMercadoProcess.Id -Force -ErrorAction SilentlyContinue
    }
    
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Ejecutando Pruebas UI..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANTE: Las pruebas tardarán aproximadamente 3-5 minutos" -ForegroundColor Yellow
Write-Host "La ventana de MicroMercado se cerrará automáticamente al finalizar" -ForegroundColor Yellow
Write-Host ""

# Ejecutar las pruebas
dotnet test PruebasUIBased/PruebasUIBased.csproj --logger "console;verbosity=normal"

$testExitCode = $LASTEXITCODE

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Pruebas Completadas" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($testExitCode -eq 0) {
    Write-Host ""
    Write-Host "? TODAS LAS PRUEBAS PASARON EXITOSAMENTE" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "?? ALGUNAS PRUEBAS FALLARON (Exit Code: $testExitCode)" -ForegroundColor Yellow
    Write-Host "Revisa los logs arriba para más detalles" -ForegroundColor Yellow
}

# Cerrar MicroMercado
Write-Host ""
Write-Host "Cerrando MicroMercado..." -ForegroundColor Yellow
if ($microMercadoProcess -and -not $microMercadoProcess.HasExited) {
    Stop-Process -Id $microMercadoProcess.Id -Force -ErrorAction SilentlyContinue
    Write-Host "[OK] MicroMercado cerrado" -ForegroundColor Green
}

Write-Host ""
Write-Host "Presiona cualquier tecla para salir..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

exit $testExitCode
