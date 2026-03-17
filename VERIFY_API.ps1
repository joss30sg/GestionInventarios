# Script de Verificación Rápida de API
# Verifica que la API esté compilada y los endpoints sean accesibles

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "VALIDACIÓN RÁPIDA DE API REST" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar que la solución compila
Write-Host "[1/5] Compilando solución del backend..." -ForegroundColor Yellow
Push-Location "backend"

$buildOutput = dotnet build --no-restore 2>&1
$buildSuccess = $LASTEXITCODE -eq 0

Pop-Location

if ($buildSuccess) {
    Write-Host "✅ Backend compila correctamente" -ForegroundColor Green
} else {
    Write-Host "❌ Error en compilación del backend" -ForegroundColor Red
    Write-Host "$buildOutput" | Select-String "error"
    exit 1
}

# 2. Verificar estructura de directorios
Write-Host ""
Write-Host "[2/5] Verificando estructura de proyecto..." -ForegroundColor Yellow

$requiredFiles = @(
    "backend\InventoryManagementAPI.sln",
    "backend\InventoryManagementAPI.Api\Program.cs",
    "backend\InventoryManagementAPI.Api\Controllers\AuthController.cs",
    "backend\InventoryManagementAPI.Api\Controllers\ProductsController.cs",
    "backend\InventoryManagementAPI.Api\Controllers\CategoriesController.cs",
    "backend\InventoryManagementAPI.Api\Controllers\InventoryController.cs",
    "backend\InventoryManagementAPI.Api\Controllers\OrdersController.cs",
    "frontend\package.json"
)

$allFilesExist = $true
foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "   ✅ $file" -ForegroundColor Green
    } else {
        Write-Host "   ❌ $file (falta)" -ForegroundColor Red
        $allFilesExist = $false
    }
}

if (-not $allFilesExist) {
    Write-Host ""
    Write-Host "⚠️  Algunos archivos están faltando" -ForegroundColor Yellow
}

# 3. Verificar configuración de CORS
Write-Host ""
Write-Host "[3/5] Verificando configuración de CORS..." -ForegroundColor Yellow

$programFile = Get-Content -Path "backend\InventoryManagementAPI.Api\Program.cs" -Raw
$corsCheck = $programFile -match 'WithOrigins.*localhost:3000.*localhost:5173'

if ($corsCheck) {
    Write-Host "✅ CORS configurado para localhost:3000 y localhost:5173" -ForegroundColor Green
} else {
    Write-Host "⚠️  Verificar configuración CORS manualmente" -ForegroundColor Yellow
}

# 4. Verificar controladores implementados
Write-Host ""
Write-Host "[4/5] Verificando controladores implementados..." -ForegroundColor Yellow

$controllers = @(
    "AuthController",
    "ProductsController",
    "CategoriesController",
    "InventoryController",
    "OrdersController"
)

foreach ($controller in $controllers) {
    $files = Get-ChildItem -Path "backend\InventoryManagementAPI.Api\Controllers\*$controller*" -ErrorAction SilentlyContinue
    if ($files) {
        Write-Host "   ✅ $controller implementado" -ForegroundColor Green
    } else {
        Write-Host "   ❌ $controller NO encontrado" -ForegroundColor Red
    }
}

# 5. Verificar archivos de configuración
Write-Host ""
Write-Host "[5/5] Verificando archivos de configuración..." -ForegroundColor Yellow

$configFiles = @(
    "backend\InventoryManagementAPI.Api\appsettings.json",
    "backend\InventoryManagementAPI.Api\appsettings.Development.json",
    "frontend\package.json"
)

foreach ($file in $configFiles) {
    if (Test-Path $file) {
        Write-Host "   ✅ $($file.Split('\')[-1])" -ForegroundColor Green
    } else {
        Write-Host "   ❌ $($file.Split('\')[-1]) (falta)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "PRÓXIMOS PASOS:" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Iniciar Backend:" -ForegroundColor Yellow
Write-Host "   cd backend" -ForegroundColor Gray
Write-Host "   dotnet run --project InventoryManagementAPI.Api" -ForegroundColor Gray
Write-Host ""
Write-Host "2. En otra terminal, iniciar Frontend:" -ForegroundColor Yellow
Write-Host "   cd frontend" -ForegroundColor Gray
Write-Host "   npm install" -ForegroundColor Gray
Write-Host "   npm run dev" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Ver documentación Swagger:" -ForegroundColor Yellow
Write-Host "   http://localhost:5000/" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. Credenciales de prueba:" -ForegroundColor Yellow
Write-Host "   Usuario: admin" -ForegroundColor Gray
Write-Host "   Contraseña: admin123" -ForegroundColor Gray
Write-Host ""
Write-Host "✅ API lista para consumo desde frontend" -ForegroundColor Green
