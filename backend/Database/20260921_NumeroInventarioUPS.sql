USE InventarioUPSDB;
GO

/*
    Agrega el número de inventario al UPS.

    - El campo queda obligatorio.
    - Los registros existentes reciben un valor temporal único:
      INV-PEND-{Id}
    - El número de inventario queda único en toda la tabla.
    - Después de ejecutar este script, edite los UPS existentes
      desde la aplicación para colocar su número de inventario real.
*/

IF COL_LENGTH('dbo.UPSs', 'NumeroInventario') IS NULL
BEGIN
    ALTER TABLE dbo.UPSs
    ADD NumeroInventario nvarchar(100) NULL;
END;
GO

UPDATE dbo.UPSs
SET NumeroInventario = CONCAT('INV-PEND-', Id)
WHERE NumeroInventario IS NULL
   OR LTRIM(RTRIM(NumeroInventario)) = '';
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.UPSs')
      AND name = 'NumeroInventario'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.UPSs
    ALTER COLUMN NumeroInventario nvarchar(100) NOT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.UPSs')
      AND name = 'IX_UPSs_NumeroInventario'
)
BEGIN
    CREATE UNIQUE INDEX IX_UPSs_NumeroInventario
        ON dbo.UPSs (NumeroInventario);
END;
GO
