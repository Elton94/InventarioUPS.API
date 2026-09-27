-- Requiere 20260927_RevisionCampos.sql. Ejecutar con la API detenida y respaldo previo.
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.UPSs', 'TipoBateriaBaseId') IS NULL
    THROW 50001, 'Ejecute primero 20260927_RevisionCampos.sql.', 1;

IF COL_LENGTH('dbo.UPSs', 'VoltajeBateria') IS NULL
    ALTER TABLE dbo.UPSs ADD VoltajeBateria decimal(9,3) NULL, CapacidadBateriaAh decimal(9,3) NULL,
        VoltajeBateriaBase decimal(9,3) NULL, CapacidadBateriaAhBase decimal(9,3) NULL;
IF COL_LENGTH('dbo.Mantenimientos', 'TipoBateriaId') IS NULL
BEGIN
    ALTER TABLE dbo.Mantenimientos ADD TipoBateriaId int NULL, VoltajeBateria decimal(9,3) NULL, CapacidadBateriaAh decimal(9,3) NULL;
    EXEC(N'UPDATE m SET TipoBateriaId = b.TipoBateriaId FROM dbo.Mantenimientos m JOIN dbo.ModelosBateria b ON b.Id = m.ModeloBateriaId;');
END;
IF COL_LENGTH('dbo.Mantenimientos', 'ResponsableUsuarioId') IS NULL
BEGIN
    ALTER TABLE dbo.Mantenimientos ADD ResponsableUsuarioId int NULL;
    -- Solo se vinculan coincidencias inequívocas. No se crean cuentas ni se adivinan identidades.
    EXEC(N'UPDATE m SET ResponsableUsuarioId = u.UsuarioId
        FROM dbo.Mantenimientos m JOIN (
            SELECT m2.Id, MIN(u2.Id) UsuarioId FROM dbo.Mantenimientos m2
            JOIN dbo.Usuarios u2 ON NULLIF(LTRIM(RTRIM(m2.TecnicoProveedor)), '''') COLLATE Latin1_General_100_CI_AS IN
                (LTRIM(RTRIM(u2.NombreCompleto)) COLLATE Latin1_General_100_CI_AS, LTRIM(RTRIM(u2.NombreUsuario)) COLLATE Latin1_General_100_CI_AS)
            GROUP BY m2.Id HAVING COUNT(DISTINCT u2.Id) = 1
        ) u ON u.Id = m.Id;');
END;
IF OBJECT_ID(N'dbo.FK_Mantenimientos_TipoBateria', N'F') IS NULL
    EXEC(N'ALTER TABLE dbo.Mantenimientos ADD CONSTRAINT FK_Mantenimientos_TipoBateria FOREIGN KEY (TipoBateriaId) REFERENCES dbo.TiposBateria(Id);');
IF OBJECT_ID(N'dbo.FK_Mantenimientos_ResponsableUsuario', N'F') IS NULL
    EXEC(N'ALTER TABLE dbo.Mantenimientos ADD CONSTRAINT FK_Mantenimientos_ResponsableUsuario FOREIGN KEY (ResponsableUsuarioId) REFERENCES dbo.Usuarios(Id);');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Mantenimientos') AND name = 'IX_Mantenimientos_TipoBateriaId')
    EXEC(N'CREATE INDEX IX_Mantenimientos_TipoBateriaId ON dbo.Mantenimientos(TipoBateriaId);');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Mantenimientos') AND name = 'IX_Mantenimientos_ResponsableUsuarioId')
    EXEC(N'CREATE INDEX IX_Mantenimientos_ResponsableUsuarioId ON dbo.Mantenimientos(ResponsableUsuarioId);');

IF OBJECT_ID(N'dbo.CostosTipoBateria', N'U') IS NULL
    CREATE TABLE dbo.CostosTipoBateria (
        Id int IDENTITY PRIMARY KEY,
        TipoBateriaId int NOT NULL REFERENCES dbo.TiposBateria(Id),
        Anio int NOT NULL CHECK (Anio BETWEEN 2020 AND 2100),
        CostoUnitario decimal(18,2) NOT NULL CHECK(CostoUnitario > 0),
        CONSTRAINT UQ_CostosTipoBateria_Anio_Tipo UNIQUE(Anio, TipoBateriaId)
    );

-- Las referencias anteriores quedan archivadas antes de retirar las columnas operativas.
IF COL_LENGTH('dbo.UPSs', 'ModeloBateriaId') IS NOT NULL AND OBJECT_ID('dbo.HistoricoBateriaUPS', 'U') IS NULL
    EXEC(N'SELECT Id AS UPSId, ModeloBateriaId, ModeloBateriaBaseId INTO dbo.HistoricoBateriaUPS FROM dbo.UPSs;');
IF COL_LENGTH('dbo.Mantenimientos', 'ModeloBateriaId') IS NOT NULL AND OBJECT_ID('dbo.HistoricoBateriaMantenimiento', 'U') IS NULL
    EXEC(N'SELECT Id AS MantenimientoId, ModeloBateriaId, ResponsableServicioId INTO dbo.HistoricoBateriaMantenimiento FROM dbo.Mantenimientos;');

DECLARE @retirar nvarchar(max) = N'';
SELECT @retirar += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(f.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(f.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(f.name) + N';'
FROM sys.foreign_keys f
WHERE EXISTS (SELECT 1 FROM sys.foreign_key_columns fc JOIN sys.columns c ON c.object_id = fc.parent_object_id AND c.column_id = fc.parent_column_id
    WHERE fc.constraint_object_id = f.object_id AND OBJECT_NAME(c.object_id) IN ('UPSs','Mantenimientos')
    AND c.name IN ('ModeloBateriaId','ModeloBateriaBaseId','ResponsableServicioId'));
EXEC sp_executesql @retirar;
SET @retirar = N'';
SELECT @retirar += N'DROP INDEX ' + QUOTENAME(i.name) + N' ON ' + QUOTENAME(OBJECT_SCHEMA_NAME(i.object_id)) + N'.' + QUOTENAME(OBJECT_NAME(i.object_id)) + N';'
FROM sys.indexes i WHERE i.is_primary_key = 0 AND i.is_unique_constraint = 0 AND EXISTS (
    SELECT 1 FROM sys.index_columns ic JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND OBJECT_NAME(c.object_id) IN ('UPSs','Mantenimientos')
    AND c.name IN ('ModeloBateriaId','ModeloBateriaBaseId','ResponsableServicioId'));
EXEC sp_executesql @retirar;
IF COL_LENGTH('dbo.UPSs','ModeloBateriaId') IS NOT NULL
    EXEC(N'ALTER TABLE dbo.UPSs DROP COLUMN ModeloBateriaId, ModeloBateriaBaseId;');
IF COL_LENGTH('dbo.Mantenimientos','ModeloBateriaId') IS NOT NULL
    EXEC(N'ALTER TABLE dbo.Mantenimientos DROP COLUMN ModeloBateriaId, ResponsableServicioId;');
IF OBJECT_ID('dbo.CostosModeloBateria','U') IS NOT NULL
    EXEC sp_rename N'dbo.CostosModeloBateria', N'HistoricoCostosModeloBateria';
IF OBJECT_ID('dbo.ModelosBateria','U') IS NOT NULL
    EXEC sp_rename N'dbo.ModelosBateria', N'HistoricoModelosBateria';
IF OBJECT_ID('dbo.ResponsablesServicio','U') IS NOT NULL
    EXEC sp_rename N'dbo.ResponsablesServicio', N'HistoricoResponsablesServicio';
COMMIT TRANSACTION;
