# 📦 Gestión de Inventarios - Guía de Inicio Rápido

Sistema integral de gestión de inventarios con autenticación basada en roles, construido con **.NET 8** y **React 18**.

---

## ⚡ Inicio Rápido (3 pasos)

### 1️⃣ **Instalar Dependencias**

#### Backend (.NET)
```bash
cd backend/InventoryManagementAPI.Api
dotnet restore
```

#### Frontend (React)
```bash
cd frontend
npm install
```

### 2️⃣ **Ejecutar la Aplicación**

**Terminal 1 - Backend:**
```bash
cd backend/InventoryManagementAPI.Api
dotnet run --configuration Debug
```
✅ Espera: `Application started. Press Ctrl+C to shut down.`  
🔗 Backend disponible en: **http://localhost:5000**

**Terminal 2 - Frontend:**
```bash
cd frontend
npm run dev
```
✅ Espera: `VITE v5.4.21 ready in XXX ms`  
🔗 Frontend disponible en: **http://localhost:3000**

### 3️⃣ **Acceder a la Aplicación**

Abre tu navegador en: **http://localhost:3000**

---

## 🔐 Credenciales de Demostración

**Solo para pruebas en desarrollo.**

### Administrador

| Campo | Valor |
|-------|-------|
| **Usuario** | `admin` |
| **Contraseña** | `Admin123!` |
| **Rol** | Administrador |
| **Acceso** | Completo al sistema |

**Permisos:**
- ✅ Gestión de usuarios
- ✅ Reportes avanzados
- ✅ Historial completo de movimientos
- ✅ Creación y edición de productos
- ✅ Acceso a todas las funcionalidades

### Empleado

| Campo | Valor |
|-------|-------|
| **Usuario** | `empleado` |
| **Contraseña** | `Empleado123!` |
| **Rol** | Empleado |
| **Acceso** | Limitado |

**Permisos:**
- ✅ Ver listado de productos
- ✅ Reportar stock bajo
- ✅ Recibir alertas de inventario
- ❌ No puede crear ni editar productos
- ❌ No puede acceder a reportes avanzados
- ❌ No puede gestionar usuarios

---

## 📋 Requisitos Previos

- **Node.js** 18+ ([Descargar](https://nodejs.org/))
- **.NET 8 SDK** ([Descargar](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
- **SQL Server Express** ([Descargar](https://www.microsoft.com/en-us/sql-server/sql-server-downloads))

Verifica la instalación:
```bash
node --version    # v18.0.0 o superior
npm --version     # 9.0.0 o superior
dotnet --version  # 8.0.0 o superior
```

### Configurar Base de Datos

La aplicación usa **SQL Server Express** con la instancia `.\SQLEXPRESS`. La base de datos se crea automáticamente al iniciar el backend por primera vez.

Si tu instancia de SQL Server tiene un nombre diferente, edita la cadena de conexión en:
```
backend/InventoryManagementAPI.Api/appsettings.json
```
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=InventoryManagementDB;Trusted_Connection=true;TrustServerCertificate=True;"
}
```

---

## 🎯 Funcionalidades Principales

### Autenticación
- ✅ Inicio de sesión con JWT
- ✅ Control de sesión basado en roles
- ✅ Persistencia de sesión en localStorage

### Roles Disponibles

#### 👨‍💼 **Administrador**
- Acceso completo al sistema
- Gestión de usuarios
- Reportes avanzados
- Historial completo de movimientos
- Creación y edición de productos

#### 👤 **Empleado**
- Ver listado de productos
- Reportar stock bajo
- Recibir alertas de inventario

### Gestión de Inventarios
- 📊 Dashboard de alertas de stock
- 🔍 Búsqueda y filtrado de productos
- 📈 Reportes de inventario
- ⚙️ Gestión de categorías y productos

---

## 📁 Estructura del Proyecto

```
.
├── backend/
│   ├── InventoryManagementAPI.Api/          # API Principal (.NET)
│   ├── InventoryManagementAPI.Application/  # Lógica de aplicación
│   ├── InventoryManagementAPI.Domain/       # Modelos de dominio
│   ├── InventoryManagementAPI.Infrastructure/ # Acceso a datos
│   └── InventoryManagementAPI.Tests/        # Tests
│
├── frontend/
│   ├── src/
│   │   ├── pages/        # Páginas (Login, Inventario, Perfil)
│   │   ├── components/   # Componentes reutilizables
│   │   ├── context/      # AuthContext para estado global
│   │   └── styles/       # Estilos globales
│   ├── package.json
│   └── vite.config.ts    # Configuración Vite
│
└── README.md             # Este archivo
```

---

## 🔧 Puertos y Direcciones

| Servicio | Puerto | URL |
|----------|--------|-----|
| Backend (.NET) | 5000 | http://localhost:5000 |
| Frontend (Vite) | 3000 | http://localhost:3000 |
| Swagger API Docs | 5000/swagger | http://localhost:5000/swagger/index.html |

---

## 🚀 Comandos Útiles

### Backend
```bash
# Compilar
dotnet build

# Ejecutar
dotnet run --configuration Debug

# Tests
dotnet test

# Migrar base de datos
dotnet ef database update
```

### Frontend
```bash
# Desarrollo
npm run dev

# Build producción
npm run build

# Tests
npm run test

# Previsualizar build
npm run preview
```

---

## 🐛 Solución de Problemas

### Puerto ya está en uso
```bash
# Windows - Matar proceso en puerto 5000
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Windows - Matar proceso en puerto 3000
netstat -ano | findstr :3000
taskkill /PID <PID> /F
```

### Base de datos no se conecta
- Verifica que SQL Server esté ejecutándose
- Checa la conexión en `backend/InventoryManagementAPI.Api/appsettings.json`

### Frontend no se compila
```bash
cd frontend
rm -r node_modules package-lock.json
npm install
npm run build
```

### CORS errors
- El backend está configurado para aceptar requests desde `http://localhost:3000`, `http://localhost:3001` y `http://localhost:5173`
- Si usas otro puerto, actualiza la configuración CORS en `backend/InventoryManagementAPI.Api/Program.cs`

---

## 🔒 Seguridad

> ⚠️ **IMPORTANTE PARA PRODUCCIÓN**
> - Las credenciales demo son SOLO para desarrollo
> - Cambiar `appsettings.json` (ConnectionString, JWT Secret)
> - Configurar HTTPS/SSL
> - Implementar base de datos segura
> - Usar variables de entorno para secretos

---

## 📝 Logs

Los logs se guardan en:
- Backend: `backend/InventoryManagementAPI.Api/logs/`
- Frontend: Consola del navegador (F12 → Console)

---

## 🤝 Soporte

Si encuentras problemas:
1. Revisa los logs
2. Verifica que los servicios estén ejecutándose
3. Comprueba los requisitos previos
4. Prueba cerrar y reabrir las aplicaciones

---

**Última actualización:** 18 de marzo de 2026  
**Versión:** 1.0.0
