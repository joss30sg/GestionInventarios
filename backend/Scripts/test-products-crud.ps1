# Script para pruebas CRUD completas de Productos
# Consume la API REST de InventoryManagementAPI
# Valida: Nombre, Descripción, Precio, Cantidad (no negativa), Categoría
# Este es un script de PRUEBA y DEMOSTRACIÓN
# Para entornos de producción, usar credenciales seguras mediante secretos

# USO:
#   .\test-products-crud.ps1                                    # Solicita contraseña interactivamente
#   .\test-products-crud.ps1 -Password (Read-Host -AsSecureString)  # Entrada segura de contraseña
#   .\test-products-crud.ps1 -Username "admin" -ApiUrl "http://localhost:5000"

param(
    [Parameter(Mandatory=$false)]
    [string]$ApiUrl = "https://localhost:7066",
    
    [Parameter(Mandatory=$false)]
    [string]$Username = "admin",
    
    [Parameter(Mandatory=$false)]
    [securestring]$Password,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipSSLValidation = $false
)

# Si NO se proporciona contraseña, solicitar de forma segura
if (-not $Password) {
    $Password = Read-Host -AsSecureString -Prompt "Ingresa la contraseña para $Username"
}

# Convertir SecureString a string (necesario para la API)
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
$PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# ============================================
# CONFIGURACIÓN INICIAL
# ============================================

if ($SkipSSLValidation) {
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
}

$global:AuthToken = ""
$global:TestResults = @{
    Passed = 0
    Failed = 0
    Skipped = 0
}

# Función para logging
function Write-Log {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Message,
        
        [Parameter(Mandatory=$false)]
        [ValidateSet("Info", "Success", "Error", "Warning")]
        [string]$Type = "Info"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    
    switch ($Type) {
        "Success" { Write-Host "[$timestamp] ✅ $Message" -ForegroundColor Green }
        "Error"   { Write-Host "[$timestamp] ❌ $Message" -ForegroundColor Red }
        "Warning" { Write-Host "[$timestamp] ⚠️  $Message" -ForegroundColor Yellow }
        default   { Write-Host "[$timestamp] ℹ️  $Message" -ForegroundColor Cyan }
    }
}

# Función para registrar resultados
function Add-TestResult {
    param(
        [Parameter(Mandatory=$true)]
        [string]$TestName,
        
        [Parameter(Mandatory=$true)]
        [ValidateSet("Passed", "Failed", "Skipped")]
        [string]$Result,
        
        [Parameter(Mandatory=$false)]
        [string]$Details = ""
    )
    
    $statusSymbol = @{
        "Passed" = "✅"
        "Failed" = "❌"
        "Skipped" = "⏭️ "
    }
    
    Write-Host "$($statusSymbol[$Result]) $TestName"
    if ($Details) {
        Write-Host "   Detalles: $Details" -ForegroundColor Gray
    }
    
    $global:TestResults.$Result++
}

# ============================================
# AUTENTICACIÓN
# ============================================

Write-Host "" -ForegroundColor Cyan
Write-Host "╔════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  PRUEBAS CRUD - GESTIÓN DE INVENTARIOS        ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Write-Log "Conectando a: $ApiUrl" "Info"
Write-Log "Autenticando como usuario: $Username" "Info"
Write-Host ""

# Intentar autenticación
try {
    $loginBody = @{
        username = $Username
        password = $PlainPassword
    } | ConvertTo-Json
    
    $loginResponse = Invoke-RestMethod `
        -Uri "$ApiUrl/api/auth/login" `
        -Method POST `
        -Headers @{ "Content-Type" = "application/json" } `
        -Body $loginBody `
        -SkipCertificateCheck
    
    if ($loginResponse.data.token) {
        $global:AuthToken = $loginResponse.data.token
        Write-Log "Autenticación exitosa" "Success"
        Write-Log "Token JWT obtenido (expira en 60 minutos)" "Info"
        Add-TestResult "Autenticación JWT" "Passed"
    } else {
        throw "No se recibió token en la respuesta"
    }
} catch {
    Write-Log "Error en autenticación: $_" "Error"
    Write-Log "Intenta con credenciales válidas o verifica que la API esté corriendo" "Error"
    Add-TestResult "Autenticación JWT" "Failed" $_.Exception.Message
    exit 1
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "PRUEBA 1: CREAR PRODUCTO (POST)"
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Headers con autenticación
$headers = @{
    "Authorization" = "Bearer $global:AuthToken"
    "Content-Type" = "application/json"
}

$productToCreate = @{
    name = "Laptop Dell XPS 13"
    description = "Laptop de negocios, procesador Intel i7, 16GB RAM, 512GB SSD"
    price = 1299.99
    quantity = 15
    categoryId = 1
} | ConvertTo-Json

Write-Log "Creando producto: Laptop Dell XPS 13" "Info"
Write-Log "Datos: Precio=1299.99, Cantidad=15, Categoría=1" "Info"

try {
    $createResponse = Invoke-RestMethod `
        -Uri "$ApiUrl/api/v1/products" `
        -Method POST `
        -Headers $headers `
        -Body $productToCreate `
        -SkipCertificateCheck
    
    if ($createResponse.success -eq $true) {
        $productId = $createResponse.data.id
        Write-Log "Producto creado exitosamente" "Success"
        Write-Log "ID del producto: $productId" "Info"
        Add-TestResult "Crear Producto (POST)" "Passed"
        Write-Host ""
    } else {
        throw "Error en respuesta: $($createResponse.message)"
    }
} catch {
    Write-Log "Error al crear producto: $_" "Error"
    Add-TestResult "Crear Producto (POST)" "Failed" $_.Exception.Message
    $productId = $null
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "PRUEBA 2: VALIDACIÓN - CANTIDAD NEGATIVA"
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$invalidProduct = @{
    name = "Producto Inválido"
    description = "Intento con cantidad negativa"
    price = 99.99
    quantity = -10
    categoryId = 1
} | ConvertTo-Json

Write-Log "Intentando crear producto con cantidad NEGATIVA (-10)" "Warning"

try {
    $invalidResponse = Invoke-RestMethod `
        -Uri "$ApiUrl/api/v1/products" `
        -Method POST `
        -Headers $headers `
        -Body $invalidProduct `
        -SkipCertificateCheck
    
    if ($invalidResponse.success -eq $false) {
        Write-Log "✓ Validación correcta: Rechaza cantidad negativa" "Success"
        Write-Log "Mensaje de error: $($invalidResponse.message)" "Info"
        Add-TestResult "Validar Cantidad Negativa" "Passed"
    }
} catch {
    $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
    if ($errorResponse.statusCode -eq 400) {
        Write-Log "✓ Validación correcta: HTTP 400 Bad Request" "Success"
        Write-Log "Mensaje: $($errorResponse.message)" "Info"
        Add-TestResult "Validar Cantidad Negativa" "Passed"
    } else {
        Write-Log "Error inesperado: $_" "Error"
        Add-TestResult "Validar Cantidad Negativa" "Failed" $_.Exception.Message
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "PRUEBA 3: LEER PRODUCTO (GET)"
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($productId) {
    Write-Log "Obteniendo producto con ID: $productId" "Info"
    
    try {
        $getResponse = Invoke-RestMethod `
            -Uri "$ApiUrl/api/v1/products/$productId" `
            -Method GET `
            -Headers $headers `
            -SkipCertificateCheck
        
        if ($getResponse.success -eq $true) {
            $product = $getResponse.data
            Write-Log "Producto obtenido exitosamente" "Success"
            Write-Host ""
            Write-Host "Datos del Producto:" -ForegroundColor Yellow
            Write-Host "  • Nombre: $($product.name)"
            Write-Host "  • Descripción: $($product.description)"
            Write-Host "  • Precio: $$($product.price)"
            Write-Host "  • Cantidad: $($product.quantity) unidades"
            Write-Host "  • Categoría ID: $($product.categoryId)"
            Write-Host "  • Fecha Creación: $($product.createdAt)"
            Write-Host ""
            Add-TestResult "Leer Producto (GET)" "Passed"
        } else {
            throw "Error en respuesta: $($getResponse.message)"
        }
    } catch {
        Write-Log "Error al obtener producto: $_" "Error"
        Add-TestResult "Leer Producto (GET)" "Failed" $_.Exception.Message
    }
} else {
    Write-Log "Se omite prueba porque producto no fue creado" "Warning"
    Add-TestResult "Leer Producto (GET)" "Skipped"
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "PRUEBA 4: LISTAR TODOS LOS PRODUCTOS (GET)"
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

Write-Log "Obteniendo lista de productos (página 1, 10 items)" "Info"

try {
    $listResponse = Invoke-RestMethod `
        -Uri "$ApiUrl/api/v1/products?page=1&pageSize=10" `
        -Method GET `
        -Headers $headers `
        -SkipCertificateCheck
    
    if ($listResponse.success -eq $true) {
        $products = $listResponse.data.items
        $totalCount = $listResponse.data.totalCount
        
        Write-Log "Lista obtenida exitosamente" "Success"
        Write-Log "Total de productos: $totalCount" "Info"
        
        if ($products.Count -gt 0) {
            Write-Host ""
            Write-Host "Primeros 5 productos:" -ForegroundColor Yellow
            $products | Select-Object -First 5 | ForEach-Object {
                Write-Host "  • $($_.name) - Precio: $$($_.price) - Stock: $($_.quantity)"
            }
            Write-Host ""
            Add-TestResult "Listar Productos (GET)" "Passed"
        } else {
            Write-Log "No hay productos registrados" "Warning"
            Add-TestResult "Listar Productos (GET)" "Passed" "Lista vacía"
        }
    } else {
        throw "Error en respuesta: $($listResponse.message)"
    }
} catch {
    Write-Log "Error al listar productos: $_" "Error"
    Add-TestResult "Listar Productos (GET)" "Failed" $_.Exception.Message
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "PRUEBA 5: ACTUALIZAR PRODUCTO (PUT)"
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($productId) {
    $updateData = @{
        name = "Laptop Dell XPS 13 - Edición 2026"
        description = "Laptop actualizada con últimos procesadores Intel"
        price = 1399.99
        quantity = 20
        categoryId = 1
    } | ConvertTo-Json
    
    Write-Log "Actualizando producto ID $productId" "Info"
    Write-Log "Nuevos datos: Precio=1399.99, Cantidad=20" "Info"
    
    try {
        $updateResponse = Invoke-RestMethod `
            -Uri "$ApiUrl/api/v1/products/$productId" `
            -Method PUT `
            -Headers $headers `
            -Body $updateData `
            -SkipCertificateCheck
        
        if ($updateResponse.success -eq $true) {
            Write-Log "Producto actualizado exitosamente" "Success"
            Add-TestResult "Actualizar Producto (PUT)" "Passed"
        } else {
            throw "Error en respuesta: $($updateResponse.message)"
        }
    } catch {
        Write-Log "Error al actualizar producto: $_" "Error"
        Add-TestResult "Actualizar Producto (PUT)" "Failed" $_.Exception.Message
    }
} else {
    Write-Log "Se omite prueba porque producto no fue creado" "Warning"
    Add-TestResult "Actualizar Producto (PUT)" "Skipped"
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "PRUEBA 6: PRUEBA DE ACCESO SIN AUTENTICACIÓN (401)"
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

Write-Log "Intentando acceder al endpoint sin token JWT" "Info"

try {
    $null = Invoke-RestMethod `
        -Uri "$ApiUrl/api/v1/products" `
        -Method GET `
        -SkipCertificateCheck
    
    Write-Log "ERROR: Debería haber rechazado la solicitud" "Error"
    Add-TestResult "Validar 401 Unauthorized" "Failed"
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Log "✓ Acceso denegado correctamente: HTTP 401 Unauthorized" "Success"
        Add-TestResult "Validar 401 Unauthorized" "Passed"
    } else {
        Write-Log "Error inesperado: $_" "Error"
        Add-TestResult "Validar 401 Unauthorized" "Failed" $_.Exception.Message
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "PRUEBA 7: DELETEAR PRODUCTO (DELETE)"
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($productId) {
    Write-Log "Eliminando producto ID $productId" "Warning"
    
    try {
        $deleteResponse = Invoke-RestMethod `
            -Uri "$ApiUrl/api/v1/products/$productId" `
            -Method DELETE `
            -Headers $headers `
            -SkipCertificateCheck
        
        if ($deleteResponse.success -eq $true) {
            Write-Log "Producto eliminado exitosamente" "Success"
            Add-TestResult "Eliminar Producto (DELETE)" "Passed"
            Write-Host ""
        } else {
            throw "Error en respuesta: $($deleteResponse.message)"
        }
    } catch {
        Write-Log "Error al eliminar producto: $_" "Error"
        Add-TestResult "Eliminar Producto (DELETE)" "Failed" $_.Exception.Message
    }
} else {
    Write-Log "Se omite prueba porque producto no fue creado" "Warning"
    Add-TestResult "Eliminar Producto (DELETE)" "Skipped"
}

# ============================================
# RESUMEN DE RESULTADOS
# ============================================

Write-Host ""
Write-Host "╔════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  RESUMEN DE PRUEBAS CRUD                       ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$total = $global:TestResults.Passed + $global:TestResults.Failed + $global:TestResults.Skipped

Write-Host "┌────────────────────────────────────┐" -ForegroundColor Cyan
Write-Host "│ ✅ Pasadas:  $($global:TestResults.Passed)" -ForegroundColor Green
Write-Host "│ ❌ Fallidas:  $($global:TestResults.Failed)" -ForegroundColor Red
Write-Host "│ ⏭️  Omitidas:  $($global:TestResults.Skipped)" -ForegroundColor Yellow
Write-Host "│ Total:       $total" -ForegroundColor Cyan
Write-Host "└────────────────────────────────────┘" -ForegroundColor Cyan

$successPercentage = if ($total -gt 0) { [math]::Round(($global:TestResults.Passed / $total) * 100, 2) } else { 0 }
Write-Host ""
Write-Host "Tasa de éxito: $successPercentage%" -ForegroundColor Cyan

# ============================================
# GUÍA DE USO DESDE FRONTEND
# ============================================

Write-Host ""
Write-Host "╔════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  EJEMPLOS DE CONSUMO DESDE FRONTEND (React)  ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Write-Host "1️⃣  AUTENTICACIÓN (Obtener JWT Token):" -ForegroundColor Yellow
Write-Host @"
const response = await fetch('https://localhost:7066/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'admin',
    password: 'Admin123!'
  })
});

const data = await response.json();
const token = data.data.token; // JWT Token para usar en siguientes requests
"@ -ForegroundColor Gray
Write-Host ""

Write-Host "2️⃣  CREAR PRODUCTO (POST):" -ForegroundColor Yellow
Write-Host @"
const createProduct = async (token) => {
  const response = await fetch('https://localhost:7066/api/v1/products', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': \`Bearer \${token}\`
    },
    body: JSON.stringify({
      name: 'Laptop Dell XPS 13',
      description: 'Laptop de negocios premium',
      price: 1299.99,
      quantity: 15,    // ⚠️  No puede ser negativa
      categoryId: 1
    })
  });
  
  const result = await response.json();
  return result.data; // { id, name, description, price, quantity, ... }
};
"@ -ForegroundColor Gray
Write-Host ""

Write-Host "3️⃣  OBTENER LISTA DE PRODUCTOS (GET):" -ForegroundColor Yellow
Write-Host @"
const getProducts = async (token, page = 1, pageSize = 10) => {
  const response = await fetch(
    \`https://localhost:7066/api/v1/products?page=\${page}&pageSize=\${pageSize}\`,
    {
      method: 'GET',
      headers: {
        'Authorization': \`Bearer \${token}\`
      }
    }
  );
  
  const result = await response.json();
  return result.data; // { items: [...], totalCount, totalPages }
};
"@ -ForegroundColor Gray
Write-Host ""

Write-Host "4️⃣  OBTENER PRODUCTO POR ID (GET):" -ForegroundColor Yellow
Write-Host @"
const getProductById = async (token, productId) => {
  const response = await fetch(
    \`https://localhost:7066/api/v1/products/\${productId}\`,
    {
      method: 'GET',
      headers: {
        'Authorization': \`Bearer \${token}\`
      }
    }
  );
  
  const result = await response.json();
  return result.data; // { id, name, description, price, quantity, ... }
};
"@ -ForegroundColor Gray
Write-Host ""

Write-Host "5️⃣  ACTUALIZAR PRODUCTO (PUT):" -ForegroundColor Yellow
Write-Host @"
const updateProduct = async (token, productId, updatedData) => {
  const response = await fetch(
    \`https://localhost:7066/api/v1/products/\${productId}\`,
    {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': \`Bearer \${token}\`
      },
      body: JSON.stringify({
        name: updatedData.name,
        description: updatedData.description,
        price: updatedData.price,
        quantity: updatedData.quantity,    // ⚠️  No puede ser negativa
        categoryId: updatedData.categoryId
      })
    }
  );
  
  const result = await response.json();
  return result.data;
};
"@ -ForegroundColor Gray
Write-Host ""

Write-Host "6️⃣  ELIMINAR PRODUCTO (DELETE):" -ForegroundColor Yellow
Write-Host @"
const deleteProduct = async (token, productId) => {
  const response = await fetch(
    \`https://localhost:7066/api/v1/products/\${productId}\`,
    {
      method: 'DELETE',
      headers: {
        'Authorization': \`Bearer \${token}\`
      }
    }
  );
  
  const result = await response.json();
  return result.success; // true o false
};
"@ -ForegroundColor Gray
Write-Host ""

Write-Host "⚠️  VALIDACIONES A CONSIDERAR EN FRONTEND:" -ForegroundColor Cyan
Write-Host @"
✓ Cantidad NUNCA debe ser negativa (la API rechazará con 400)
✓ Precio debe ser positivo
✓ Nombre y descripción son requeridos
✓ Categoría debe existir en el sistema
✓ El token JWT expira en 60 minutos
✓ Siempre incluir header: Authorization: Bearer {token}
✓ Manejar los 3 tipos de error:
  - 400: Validación fallida (datos inválidos)
  - 401: No autenticado (token inválido/expirado)
  - 403: No autorizado (sin permisos para la operación)
"@ -ForegroundColor Yellow

Write-Host ""
Write-Host "✅ Script de pruebas completado" -ForegroundColor Green
