-- Ejecutar después de 20260927_AdministracionCatalogos.sql, con la API detenida.
-- Conserva los datos y nombres históricos. Se puede volver a ejecutar.
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.AuditoriaCatalogos', N'U') IS NULL
    THROW 50001, 'Ejecute primero 20260927_AdministracionCatalogos.sql.', 1;

IF COL_LENGTH('dbo.UPSs', 'TipoBateriaId') IS NULL
BEGIN
    ALTER TABLE dbo.UPSs ADD TipoBateriaId int NULL;
    EXEC(N'UPDATE u SET TipoBateriaId = m.TipoBateriaId FROM dbo.UPSs u JOIN dbo.ModelosBateria m ON m.Id = u.ModeloBateriaId;');
END;
IF COL_LENGTH('dbo.UPSs', 'TipoBateriaBaseId') IS NULL
BEGIN
    ALTER TABLE dbo.UPSs ADD TipoBateriaBaseId int NULL;
    EXEC(N'UPDATE u SET TipoBateriaBaseId = m.TipoBateriaId FROM dbo.UPSs u JOIN dbo.ModelosBateria m ON m.Id = u.ModeloBateriaBaseId;');
END;
IF OBJECT_ID(N'dbo.FK_UPSs_TiposBateria_TipoBateriaId', N'F') IS NULL
    EXEC(N'ALTER TABLE dbo.UPSs ADD CONSTRAINT FK_UPSs_TiposBateria_TipoBateriaId FOREIGN KEY (TipoBateriaId) REFERENCES dbo.TiposBateria(Id);');
IF OBJECT_ID(N'dbo.FK_UPSs_TiposBateria_TipoBateriaBaseId', N'F') IS NULL
    EXEC(N'ALTER TABLE dbo.UPSs ADD CONSTRAINT FK_UPSs_TiposBateria_TipoBateriaBaseId FOREIGN KEY (TipoBateriaBaseId) REFERENCES dbo.TiposBateria(Id);');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.UPSs') AND name = 'IX_UPSs_TipoBateriaId')
    EXEC(N'CREATE INDEX IX_UPSs_TipoBateriaId ON dbo.UPSs(TipoBateriaId);');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.UPSs') AND name = 'IX_UPSs_TipoBateriaBaseId')
    EXEC(N'CREATE INDEX IX_UPSs_TipoBateriaBaseId ON dbo.UPSs(TipoBateriaBaseId);');

IF OBJECT_ID(N'dbo.ResponsablesServicio', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ResponsablesServicio (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ResponsablesServicio PRIMARY KEY,
        Nombre nvarchar(150) COLLATE Latin1_General_100_CI_AS NOT NULL,
        Activo bit NOT NULL CONSTRAINT DF_ResponsablesServicio_Activo DEFAULT 1,
        CONSTRAINT UQ_ResponsablesServicio_Nombre UNIQUE(Nombre)
    );
END;
IF COL_LENGTH('dbo.Mantenimientos', 'ResponsableServicioId') IS NULL
BEGIN
    ALTER TABLE dbo.Mantenimientos ADD ResponsableServicioId int NULL;
    INSERT INTO dbo.ResponsablesServicio (Nombre)
    SELECT DISTINCT LTRIM(RTRIM(m.TecnicoProveedor)) COLLATE Latin1_General_100_CI_AS
    FROM dbo.Mantenimientos m
    WHERE NULLIF(LTRIM(RTRIM(m.TecnicoProveedor)), '') IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.ResponsablesServicio r WHERE r.Nombre = LTRIM(RTRIM(m.TecnicoProveedor)) COLLATE Latin1_General_100_CI_AS);
    EXEC(N'UPDATE m SET ResponsableServicioId = r.Id FROM dbo.Mantenimientos m JOIN dbo.ResponsablesServicio r ON r.Nombre = LTRIM(RTRIM(m.TecnicoProveedor)) COLLATE Latin1_General_100_CI_AS;');
END;
IF OBJECT_ID(N'dbo.FK_Mantenimientos_ResponsablesServicio', N'F') IS NULL
    EXEC(N'ALTER TABLE dbo.Mantenimientos ADD CONSTRAINT FK_Mantenimientos_ResponsablesServicio FOREIGN KEY (ResponsableServicioId) REFERENCES dbo.ResponsablesServicio(Id);');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Mantenimientos') AND name = 'IX_Mantenimientos_ResponsableServicioId')
    EXEC(N'CREATE INDEX IX_Mantenimientos_ResponsableServicioId ON dbo.Mantenimientos(ResponsableServicioId);');
COMMIT TRANSACTION;
