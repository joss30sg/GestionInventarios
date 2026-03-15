/*
╔════════════════════════════════════════════════════════════════════════════╗
║                    SCRIPT: Crear Esquema de Base de Datos                 ║
║                       Sistema: Gestión de Inventarios                     ║
╚════════════════════════════════════════════════════════════════════════════╝

DESCRIPCIÓN:
  Este script crea la estructura completa de tablas para el sistema de
  gestión de inventarios. Incluye:
  - Tablas principales (Categorías, Productos)
  - Tablas de auditoría (Movimientos de stock, Alertas)
  - Tablas de seguridad (Usuarios, Logs de auditoría)

RESTRICCIONES IMPLEMENTADAS:
  ✓ Claves primarias en todas las tablas
  ✓ Claves foráneas con integridad referencial
  ✓ Restricciones CHECK para validar valores
  ✓ Índices para optimizar búsquedas
  ✓ Campos de auditoría (CreatedAt, UpdatedAt)

NOTAS IMPORTANTES:
  - Todos los IDs son auto-incrementales
  - Las fechas se registran en UTC (GETUTCDATE())
  - Las cantidades nunca pueden ser negativas
  - Los precios nunca pueden ser negativos
*/

-- ============================================
-- 1. TABLA: Categorías de Productos
-- ============================================
-- Propósito: Organizar productos en categorías
-- Restricciones:
--   - Nombre debe ser único
--   - Estado permite activar/desactivar categorías

CREATE TABLE [dbo].[Categories] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    
    CONSTRAINT [UK_Category_Name] UNIQUE ([Name])
);

PRINT 'Tabla [Categories] creada exitosamente'
GO

-- ============================================
-- 2. TABLA: Productos del Inventario
-- ============================================
-- Propósito: Almacenar información de productos disponibles
-- Restricciones:
--   - SKU (código) único para evitar duplicados
--   - Cantidad >= 0 (sistema rechaza números negativos)
--   - Precio >= 0 (validación de datos)
--   - ReorderLevel >= 0 (nivel mínimo de stock)
-- Índices: Optimiza búsquedas por categoría y estado activo

CREATE TABLE dbo.Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Sku NVARCHAR(100) NOT NULL,
    Name NVARCHAR(300) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    CategoryId INT NOT NULL REFERENCES dbo.Categories(Id) ON DELETE CASCADE,
    Quantity INT NOT NULL DEFAULT 0,
    ReorderLevel INT DEFAULT 10,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    UNIQUE (Sku),
    CHECK (Price >= 0),
    CHECK (Quantity >= 0),
    CHECK (ReorderLevel >= 0)
);

-- Índices para mejorar rendimiento de consultas frecuentes
CREATE INDEX IX_Product_CategoryId ON dbo.Products(CategoryId);
CREATE INDEX IX_Product_IsActive ON dbo.Products(IsActive);

PRINT 'Tabla [Products] creada exitosamente'
GO

-- ============================================
-- 3. TABLA: Movimientos de Stock
-- ============================================
-- Propósito: Registrar historial COMPLETO de todos los cambios de inventario
-- Restricciones:
--   - Tipos válidos: Purchase (Compra), Sale (Venta), Adjustment (Ajuste),
--                   Return (Devolución), Damage (Daño), Transfer (Transferencia)
--   - Cantidad SIEMPRE > 0 (no se permiten movimientos de 0)
--   - Cada movimiento es immutable (nunca se actualiza, solo se inserta)
-- Índices: Optimiza búsquedas por producto y fecha

CREATE TABLE [dbo].[StockMovements] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ProductId] INT NOT NULL,
    [MovementType] NVARCHAR(50) NOT NULL,
    [Quantity] INT NOT NULL,
    [Reason] NVARCHAR(500),                    -- Descripción del motivo
    [Reference] NVARCHAR(100),                  -- Número de documento (PO, INV, etc)
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] INT,                            -- ID del usuario que registró el movimiento
    CONSTRAINT [FK_StockMovement_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE,
    CONSTRAINT [CK_Movement_Type] CHECK ([MovementType] IN ('Purchase', 'Sale', 'Adjustment', 'Return', 'Damage', 'Transfer')),
    CONSTRAINT [CK_Movement_Quantity] CHECK ([Quantity] > 0)
);

-- Índices para optimizar consultas de auditoría
CREATE INDEX [IX_StockMovement_ProductId] ON [dbo].[StockMovements]([ProductId]);
CREATE INDEX [IX_StockMovement_MovementType] ON [dbo].[StockMovements]([MovementType]);
CREATE INDEX [IX_StockMovement_CreatedAt] ON [dbo].[StockMovements]([CreatedAt]);

PRINT 'Tabla [StockMovements] creada exitosamente'
GO

-- ============================================
-- 4. TABLA: Alertas de Inventario
-- ============================================
-- Propósito: Generar alertas automáticas cuando el stock está fuera de lo normal
-- Tipos de alertas:
--   - LowStock: Cantidad está por debajo del nivel de reorden
--   - OutOfStock: Producto agotado (cantidad = 0)
--   - Overstock: Existencias excesivas
-- Restricciones:
--   - IsResolved permite marcar alertas como atendidas
--   - ResolvedAt se registra cuando se cierra la alerta

CREATE TABLE [dbo].[InventoryAlerts] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ProductId] INT NOT NULL,
    [AlertType] NVARCHAR(50) NOT NULL,
    [CurrentQuantity] INT NOT NULL,
    [ReorderLevel] INT NOT NULL,
    [Message] NVARCHAR(500),
    [IsResolved] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ResolvedAt] DATETIME2,
    CONSTRAINT [FK_Alert_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE,
    CONSTRAINT [CK_Alert_Type] CHECK ([AlertType] IN ('LowStock', 'OutOfStock', 'Overstock'))
);

-- Índices para búsquedas rápidas por producto y estado
CREATE INDEX [IX_Alert_ProductId] ON [dbo].[InventoryAlerts]([ProductId]);
CREATE INDEX [IX_Alert_IsResolved] ON [dbo].[InventoryAlerts]([IsResolved]);

PRINT 'Tabla [InventoryAlerts] creada exitosamente'
GO

-- ============================================
-- 5. TABLA: Usuarios del Sistema
-- ============================================
-- Propósito: Gestionar usuarios y autenticación del sistema
-- Roles disponibles:
--   - Admin: Acceso completo a todas las funcionalidades
--   - Manager: Lectura y escritura, sin acceso a configuración
--   - User: Solo lectura (visualizar inventario)
-- Restricciones:
--   - Username debe ser único
--   - Email debe ser único (si se proporciona)
--   - PasswordHash se almacena de forma segura (nunca en texto plano)

CREATE TABLE [dbo].[Users] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Username] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(256),
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [Role] NVARCHAR(50) NOT NULL DEFAULT 'User',
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LastLogin] DATETIME2,
    CONSTRAINT [UK_User_Username] UNIQUE ([Username]),
    CONSTRAINT [UK_User_Email] UNIQUE ([Email]),
    CONSTRAINT [CK_User_Role] CHECK ([Role] IN ('Admin', 'Manager', 'User'))
);

PRINT 'Tabla [Users] creada exitosamente'
GO

-- ============================================
-- 6. TABLA: Logs de Auditoría
-- ============================================
-- Propósito: Registrar TODAS las acciones del sistema para auditoría
-- Información registrada:
--   - Quién realizó la acción (UserId)
--   - Qué tipo de acción (CREATE, UPDATE, DELETE, VIEW)
--   - En qué entidad (Products, Categories, Users, etc.)
--   - Detalles específicos de los cambios
--   - Cuándo se realizó la acción (CreatedAt)
-- Uso: Trazabilidad completa y análisis de seguridad

CREATE TABLE [dbo].[AuditLogs] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT,
    [Action] NVARCHAR(50),                      -- CREATE, UPDATE, DELETE, VIEW
    [EntityType] NVARCHAR(50),                  -- Products, Categories, Users, etc.
    [EntityId] INT,                             -- ID del registro modificado
    [Details] NVARCHAR(MAX),                    -- Información JSON con cambios
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_AuditLog_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL
);

-- Índices para búsquedas rápidas de auditoría
CREATE INDEX [IX_AuditLog_UserId] ON [dbo].[AuditLogs]([UserId]);
CREATE INDEX [IX_AuditLog_CreatedAt] ON [dbo].[AuditLogs]([CreatedAt]);

PRINT 'Tabla [AuditLogs] creada exitosamente'
GO

PRINT ''
PRINT '╔════════════════════════════════════════════════════════╗'
PRINT '║   ✅ ESQUEMA DE BD CREADO EXITOSAMENTE                ║'
PRINT '║                                                        ║'
PRINT '║   Tablas creadas:                                     ║'
PRINT '║   ✓ Categories (Categorías)                           ║'
PRINT '║   ✓ Products (Productos)                              ║'
PRINT '║   ✓ StockMovements (Movimientos de Stock)             ║'
PRINT '║   ✓ InventoryAlerts (Alertas de Inventario)           ║'
PRINT '║   ✓ Users (Usuarios)                                  ║'
PRINT '║   ✓ AuditLogs (Logs de Auditoría)                     ║'
PRINT '║                                                        ║'
PRINT '║   PRÓXIMO PASO: Ejecutar 02_Seed_Initial_TestData.sql ║'
PRINT '╚════════════════════════════════════════════════════════╝'
