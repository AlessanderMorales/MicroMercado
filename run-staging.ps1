# ?? Script para ejecutar MicroMercado en Staging
# Uso: .\run-staging.ps1

Write-Host "?? Configurando entorno Staging..." -ForegroundColor Cyan

# Cambiar al directorio del proyecto
Set-Location "MicroMercado"

# Configurar variable de entorno
$env:ASPNETCORE_ENVIRONMENT = "Staging"

Write-Host "? Entorno configurado: Staging" -ForegroundColor Green
Write-Host "? Puerto HTTPS: 7155" -ForegroundColor Green
Write-Host "? Puerto HTTP: 5156" -ForegroundColor Green
Write-Host ""
Write-Host "?? Iniciando servidor..." -ForegroundColor Cyan
Write-Host ""

# Ejecutar la aplicación con URLs específicas
dotnet run --urls "https://localhost:7155;http://localhost:5156"
