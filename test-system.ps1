#!/usr/bin/env powershell
# Script de Prueba Rápida - Verificación de Funcionalidad
# Ejecutar: .\test-system.ps1

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "  🧪 PRUEBA DE SISTEMA COMPLETO" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

# Colores para mensajes
function Show-Success {
    param([string]$message)
    Write-Host "  ✅ $message" -ForegroundColor Green
}

function Show-Error {
    param([string]$message)
    Write-Host "  ❌ $message" -ForegroundColor Red
}

function Show-Info {
    param([string]$message)
    Write-Host "  ℹ️  $message" -ForegroundColor Cyan
}

# 1. Verificar que los servidores estén corriendo
Write-Host ""
Write-Host "1️⃣  Verificando servidores..." -ForegroundColor Yellow

# Backend
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -Method Get -ErrorAction SilentlyContinue -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Show-Success "Backend API (.NET) respondiendo en puerto 5000"
    } else {
        Show-Error "Backend respondió pero con código $($response.StatusCode)"
    }
} catch {
    Show-Error "Backend no responde en puerto 5000"
}

# Frontend
try {
    $response = Invoke-WebRequest -Uri "http://localhost:3000/" -Method Get -ErrorAction SilentlyContinue -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Show-Success "Frontend React respondiendo en puerto 3000"
    } else {
        Show-Error "Frontend respondió pero con código $($response.StatusCode)"
    }
} catch {
    Show-Error "Frontend no responde en puerto 3000"
}

# 2. Verificar Swagger
Write-Host ""
Write-Host "2️⃣  Verificando documentación Swagger..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/index.html" -Method Get -ErrorAction SilentlyContinue -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Show-Success "Swagger UI disponible en http://localhost:5000/index.html"
    }
} catch {
    Show-Error "Swagger UI no disponible"
}

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api-docs/v1/swagger.json" -Method Get -ErrorAction SilentlyContinue -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Show-Success "Swagger JSON Schema disponible"
        $json = $response.Content | ConvertFrom-Json
        Show-Info "API: $($json.info.title) v$($json.info.version)"
    }
} catch {
    Show-Error "Swagger JSON no disponible"
}

# 3. Probar Endpoints Sin Autenticación
Write-Host ""
Write-Host "3️⃣  Probando endpoints públicos..." -ForegroundColor Yellow

# Register - Usuarios públicos pueden registrarse
$registerData = @{
    username = "testuser_$(Get-Random)"
    email = "test_$(Get-Random)@test.com"
    password = "TestPass@123"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/register" `
        -Method Post `
        -ContentType "application/json" `
        -Body $registerData `
        -ErrorAction SilentlyContinue `
        -UseBasicParsing
    
    if ($response.StatusCode -eq 200) {
        Show-Success "Endpoint POST /api/auth/register ✅"
    } else {
        Show-Info "Registro respondió con $($response.StatusCode)"
    }
} catch {
    Show-Info "Registro puede fallar por validación (esperado)"
}

# 4. Probar Login
Write-Host ""
Write-Host "4️⃣  Probando autenticación..." -ForegroundColor Yellow

$loginData = @{
    username = "admin"
    password = "admin123"
} | ConvertTo-Json

$token = $null
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" `
        -Method Post `
        -ContentType "application/json" `
        -Body $loginData `
        -ErrorAction SilentlyContinue `
        -UseBasicParsing
    
    if ($response.StatusCode -eq 200) {
        Show-Success "Login exitoso con usuario 'admin'"
        $data = $response.Content | ConvertFrom-Json
        if ($data.token) {
            $token = $data.token
            Show-Success "JWT Token obtenido"
        }
    }
} catch {
    Show-Error "Error en login: $($_.Exception.Message)"
}

# 5. Probar Endpoints Protegidos
if ($token) {
    Write-Host ""
    Write-Host "5️⃣  Probando endpoints protegidos..." -ForegroundColor Yellow
    
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    # Get My Orders
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/api/orders" `
            -Method Get `
            -Headers $headers `
            -ErrorAction SilentlyContinue `
            -UseBasicParsing
        
        if ($response.StatusCode -eq 200) {
            Show-Success "GET /api/orders (Mis pedidos) ✅"
            $data = $response.Content | ConvertFrom-Json
            Show-Info "Pedidos encontrados: $($data.Count)"
        }
    } catch {
        Show-Error "Error al obtener pedidos"
    }
    
    # Get All Orders (Admin endpoint)
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/api/orders/admin/all" `
            -Method Get `
            -Headers $headers `
            -ErrorAction SilentlyContinue `
            -UseBasicParsing
        
        if ($response.StatusCode -eq 200) {
            Show-Success "GET /api/orders/admin/all (Todos los pedidos) ✅"
            $data = $response.Content | ConvertFrom-Json
            Show-Info "Total de pedidos en sistema: $($data.Count)"
        }
    } catch {
        Show-Error "Error al obtener todos los pedidos"
    }
}

# 6. Resumen
Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "  ✨ RESUMEN DE PRUEBAS" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Próximas acciones:" -ForegroundColor Yellow
Write-Host "  1. Abre http://localhost:3000 para usar la aplicación"
Write-Host "  2. Abre http://localhost:5000/index.html para ver Swagger"
Write-Host "  3. Prueba las siguientes cuentas:" 
Write-Host "     - Usuario: admin / admin123 (rol Admin)"
Write-Host "     - Usuario: empleado / empleado123 (rol User)"
Write-Host ""
Write-Host "🎯 Funcionalidades para probar:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Como USUARIO EMPLEADO (empleado):"
Write-Host "    ✓ Ver todos los pedidos"
Write-Host "    ✓ Crear un nuevo pedido"
Write-Host "    ✓ Editar un pedido"
Write-Host "    ✓ Eliminar un pedido"
Write-Host ""
Write-Host "  Como ADMIN (admin):"
Write-Host "    ✓ Clickear pestaña Gestión de Pedidos"
Write-Host "    ✓ Ver todos los pedidos del sistema"
Write-Host "    ✓ Filtrar por estado (Pendientes, Aprobados, etc)"
Write-Host "    ✓ APROBAR un pedido"
Write-Host "    ✓ RECHAZAR un pedido con razón"
Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
