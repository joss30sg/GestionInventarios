Write-Host "======================================================"
Write-Host "    TEST DE SISTEMA COMPLETO"
Write-Host "======================================================"
Write-Host ""

# 1. Verificar que los servidores esten corriendo
Write-Host "1. Verificando servidores..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -Method Get -ErrorAction SilentlyContinue -UseBasicParsing
    Write-Host "   OK: Backend API en puerto 5000" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: Backend no responde en puerto 5000" -ForegroundColor Red
}

try {
    $response = Invoke-WebRequest -Uri "http://localhost:3000/" -Method Get -ErrorAction SilentlyContinue -UseBasicParsing
    Write-Host "   OK: Frontend React en puerto 3000" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: Frontend no responde en puerto 3000" -ForegroundColor Red
}

# 2. Verificar Swagger
Write-Host ""
Write-Host "2. Verificando Swagger..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/index.html" -Method Get -ErrorAction SilentlyContinue -UseBasicParsing
    Write-Host "   OK: Swagger UI disponible en http://localhost:5000/index.html" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: Swagger no disponible" -ForegroundColor Red
}

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api-docs/v1/swagger.json" -Method Get -ErrorAction SilentlyContinue -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        $json = $response.Content | ConvertFrom-Json
        Write-Host "   OK: Swagger JSON Schema disponible" -ForegroundColor Green
        Write-Host "       API: $($json.info.title) v$($json.info.version)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ERROR: Swagger JSON no disponible" -ForegroundColor Red
}

# 3. Probar Login
Write-Host ""
Write-Host "3. Probando autenticacion con 'admin'..." -ForegroundColor Yellow

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
        Write-Host "   OK: Login exitoso" -ForegroundColor Green
        $data = $response.Content | ConvertFrom-Json
        if ($data.token) {
            $token = $data.token
            Write-Host "   OK: JWT Token obtenido" -ForegroundColor Green
        }
    }
} catch {
    Write-Host "   ERROR: Problema en login" -ForegroundColor Red
}

# 4. Probar Endpoints Protegidos
if ($token) {
    Write-Host ""
    Write-Host "4. Probando endpoints protegidos..." -ForegroundColor Yellow
    
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/api/orders" `
            -Method Get `
            -Headers $headers `
            -ErrorAction SilentlyContinue `
            -UseBasicParsing
        
        if ($response.StatusCode -eq 200) {
            Write-Host "   OK: GET /api/orders (Mis pedidos)" -ForegroundColor Green
            $data = $response.Content | ConvertFrom-Json
            Write-Host "       Pedidos encontrados: $($data.Count)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "   ERROR: Problema al obtener pedidos" -ForegroundColor Red
    }
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/api/orders/admin/all" `
            -Method Get `
            -Headers $headers `
            -ErrorAction SilentlyContinue `
            -UseBasicParsing
        
        if ($response.StatusCode -eq 200) {
            Write-Host "   OK: GET /api/orders/admin/all (Todos)" -ForegroundColor Green
            $data = $response.Content | ConvertFrom-Json
            Write-Host "       Total de pedidos: $($data.Count)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "   ERROR: Problema en endpoint admin" -ForegroundColor Red
    }
}

# 5. Resumen
Write-Host ""
Write-Host "======================================================"
Write-Host "RESUMEN"
Write-Host "======================================================"
Write-Host ""
Write-Host "Direcciones importantes:"
Write-Host "  - Aplicacion:  http://localhost:3000"
Write-Host "  - API:         http://localhost:5000/api"
Write-Host "  - Swagger:     http://localhost:5000/index.html"
Write-Host ""
Write-Host "Usuarios de prueba:"
Write-Host "  - admin / admin123 (rol Admin)"
Write-Host "  - empleado / empleado123 (rol User)"
Write-Host ""
Write-Host "Funcionalidades a probar:"
Write-Host "  USUARIO: Crear pedido, Ver pedidos, Editar, Eliminar"
Write-Host "  ADMIN: Ver todos, Filtrar, Aprobar, Rechazar con razon"
Write-Host ""
Write-Host "======================================================"
Write-Host ""
