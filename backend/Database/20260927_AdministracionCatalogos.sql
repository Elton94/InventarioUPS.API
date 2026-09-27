-- Requiere 20260927_CatalogoBaterias.sql. Detenga la API y seleccione su base.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.ModelosBateria', 'U') IS NULL
    THROW 50001, 'Ejecute primero 20260927_CatalogoBaterias.sql.', 1;
IF COL_LENGTH('dbo.TiposBateria', 'Activo') IS NULL
    ALTER TABLE dbo.TiposBateria ADD Activo bit NOT NULL CONSTRAINT DF_TiposBateria_Activo DEFAULT 1;
IF COL_LENGTH('dbo.ModelosBateria', 'Activo') IS NULL
    ALTER TABLE dbo.ModelosBateria ADD Activo bit NOT NULL CONSTRAINT DF_ModelosBateria_Activo DEFAULT 1;
IF COL_LENGTH('dbo.Ciudades', 'EsLocal') IS NULL
BEGIN
    ALTER TABLE dbo.Ciudades ADD EsLocal bit NOT NULL CONSTRAINT DF_Ciudades_EsLocal DEFAULT 0;
    EXEC(N'UPDATE dbo.Ciudades SET EsLocal = 1 WHERE LTRIM(RTRIM(Nombre)) COLLATE Latin1_General_100_CI_AS IN (N''San Pedro Sula'', N''La Ceiba'', N''Distrito Central'');');
END;
IF OBJECT_ID('dbo.MarcasUPS', 'U') IS NULL
    CREATE TABLE dbo.MarcasUPS (Id int IDENTITY PRIMARY KEY, Nombre nvarchar(100) COLLATE Latin1_General_100_CI_AS NOT NULL UNIQUE, Activo bit NOT NULL DEFAULT 1);
IF OBJECT_ID('dbo.ModelosUPS', 'U') IS NULL
    CREATE TABLE dbo.ModelosUPS (Id int IDENTITY PRIMARY KEY, Nombre nvarchar(100) COLLATE Latin1_General_100_CI_AS NOT NULL,
        Activo bit NOT NULL DEFAULT 1, MarcaUPSId int NOT NULL REFERENCES dbo.MarcasUPS(Id), CONSTRAINT UQ_ModelosUPS_Marca_Nombre UNIQUE(MarcaUPSId, Nombre));
IF COL_LENGTH('dbo.UPSs', 'ModeloUPSId') IS NULL
    ALTER TABLE dbo.UPSs ADD ModeloUPSId int NULL REFERENCES dbo.ModelosUPS(Id);
INSERT INTO dbo.MarcasUPS(Nombre)
SELECT DISTINCT LTRIM(RTRIM(u.Marca)) COLLATE Latin1_General_100_CI_AS FROM dbo.UPSs u
WHERE LTRIM(RTRIM(u.Marca)) <> '' AND NOT EXISTS (SELECT 1 FROM dbo.MarcasUPS m WHERE m.Nombre = LTRIM(RTRIM(u.Marca)) COLLATE Latin1_General_100_CI_AS);
INSERT INTO dbo.ModelosUPS(Nombre, MarcaUPSId)
SELECT DISTINCT LTRIM(RTRIM(u.Modelo)) COLLATE Latin1_General_100_CI_AS, m.Id FROM dbo.UPSs u
JOIN dbo.MarcasUPS m ON m.Nombre = LTRIM(RTRIM(u.Marca)) COLLATE Latin1_General_100_CI_AS
WHERE LTRIM(RTRIM(u.Modelo)) <> '' AND NOT EXISTS (SELECT 1 FROM dbo.ModelosUPS x WHERE x.MarcaUPSId = m.Id AND x.Nombre = LTRIM(RTRIM(u.Modelo)) COLLATE Latin1_General_100_CI_AS);
EXEC(N'UPDATE u SET ModeloUPSId = x.Id, Marca = m.Nombre, Modelo = x.Nombre FROM dbo.UPSs u JOIN dbo.MarcasUPS m ON m.Nombre = LTRIM(RTRIM(u.Marca)) COLLATE Latin1_General_100_CI_AS JOIN dbo.ModelosUPS x ON x.MarcaUPSId = m.Id AND x.Nombre = LTRIM(RTRIM(u.Modelo)) COLLATE Latin1_General_100_CI_AS WHERE u.ModeloUPSId IS NULL;');
IF OBJECT_ID('dbo.AuditoriaCatalogos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditoriaCatalogos (Id int IDENTITY PRIMARY KEY, Fecha datetime2 NOT NULL, UsuarioId int NOT NULL,
        Usuario nvarchar(200) NOT NULL, Catalogo nvarchar(50) NOT NULL, RegistroId int NOT NULL, Accion nvarchar(30) NOT NULL,
        Antes nvarchar(max) NOT NULL, Despues nvarchar(max) NOT NULL);
    CREATE INDEX IX_AuditoriaCatalogos_Fecha ON dbo.AuditoriaCatalogos(Fecha);
END;
-- Índices adicionales protegen nombres sin distinguir mayúsculas o espacios exteriores.
-- Los duplicados preexistentes provocan rollback; no se fusionan ni eliminan registros.
IF COL_LENGTH('dbo.Departamentos', 'NombreCatalogoNormalizado') IS NULL
    EXEC(N'ALTER TABLE dbo.Departamentos ADD NombreCatalogoNormalizado AS (UPPER(LTRIM(RTRIM(Nombre))) COLLATE Latin1_General_100_CI_AS) PERSISTED; CREATE UNIQUE INDEX UX_Departamentos_Catalogo ON dbo.Departamentos(NombreCatalogoNormalizado);');
IF COL_LENGTH('dbo.Ciudades', 'NombreCatalogoNormalizado') IS NULL
    EXEC(N'ALTER TABLE dbo.Ciudades ADD NombreCatalogoNormalizado AS (UPPER(LTRIM(RTRIM(Nombre))) COLLATE Latin1_General_100_CI_AS) PERSISTED; CREATE UNIQUE INDEX UX_Ciudades_Catalogo ON dbo.Ciudades(DepartamentoId, NombreCatalogoNormalizado);');
IF COL_LENGTH('dbo.TiposBateria', 'NombreCatalogoNormalizado') IS NULL
    EXEC(N'ALTER TABLE dbo.TiposBateria ADD NombreCatalogoNormalizado AS (UPPER(LTRIM(RTRIM(Nombre))) COLLATE Latin1_General_100_CI_AS) PERSISTED; CREATE UNIQUE INDEX UX_TiposBateria_Catalogo ON dbo.TiposBateria(NombreCatalogoNormalizado);');
IF NOT EXISTS (SELECT 1 FROM dbo.Permisos WHERE Codigo = 'GESTIONAR_CATALOGOS')
BEGIN
    INSERT dbo.Permisos(Codigo, Nombre, Descripcion, Activo, FechaCreacion)
    VALUES ('GESTIONAR_CATALOGOS', N'Gestionar catálogos', N'Administrar catálogos y consultar su historial de cambios.', 1, GETDATE());
    DECLARE @permisoCatalogos int = SCOPE_IDENTITY();
    INSERT dbo.RolPermisos(RolId, PermisoId, FechaCreacion)
    SELECT Id, @permisoCatalogos, GETDATE() FROM dbo.Roles WHERE Nombre IN (N'Control Total', N'Administrador');
END;
COMMIT;
PRINT N'Catálogos actualizados. Reinicie la aplicación y vuelva a iniciar sesión para cargar el permiso.';
