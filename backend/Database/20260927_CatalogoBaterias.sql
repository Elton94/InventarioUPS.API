-- Ejecutar en la base de Inventario UPS antes de iniciar la API actualizada.
-- Reejecutable. Conserva los registros anteriores sin inventar su modelo de batería.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.TiposBateria', 'U') IS NULL
    CREATE TABLE dbo.TiposBateria (Id int IDENTITY PRIMARY KEY, Nombre nvarchar(80) NOT NULL UNIQUE);
IF OBJECT_ID('dbo.ModelosBateria', 'U') IS NULL
    CREATE TABLE dbo.ModelosBateria (
        Id int IDENTITY PRIMARY KEY,
        TipoBateriaId int NOT NULL REFERENCES dbo.TiposBateria(Id),
        Nombre nvarchar(150) COLLATE Latin1_General_100_CI_AS NOT NULL,
        CONSTRAINT UQ_ModelosBateria_Tipo_Nombre UNIQUE(TipoBateriaId, Nombre));
IF OBJECT_ID('dbo.CostosModeloBateria', 'U') IS NULL
    CREATE TABLE dbo.CostosModeloBateria (
        Id int IDENTITY PRIMARY KEY,
        ModeloBateriaId int NOT NULL REFERENCES dbo.ModelosBateria(Id),
        Anio int NOT NULL CHECK(Anio BETWEEN 2020 AND 2100),
        CostoUnitario decimal(18,2) NOT NULL CHECK(CostoUnitario > 0),
        CONSTRAINT UQ_CostosModeloBateria_Anio_Modelo UNIQUE(Anio, ModeloBateriaId));
IF NOT EXISTS (SELECT 1 FROM dbo.TiposBateria)
INSERT INTO dbo.TiposBateria(Nombre)
SELECT v.Nombre FROM (VALUES (N'Plomo-ácido VRLA AGM'), (N'Plomo-ácido gel'),
    (N'Plomo-ácido inundada'), (N'Ion de litio'), (N'Litio ferrofosfato (LiFePO4)'),
    (N'Níquel-cadmio (Ni-Cd)'), (N'Níquel-hidruro metálico (Ni-MH)'), (N'Otro')) v(Nombre)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TiposBateria t WHERE t.Nombre = v.Nombre);
IF COL_LENGTH('dbo.UPSs', 'ModeloBateriaId') IS NULL
    ALTER TABLE dbo.UPSs ADD ModeloBateriaId int NULL REFERENCES dbo.ModelosBateria(Id);
IF COL_LENGTH('dbo.UPSs', 'ModeloBateriaBaseId') IS NULL
    ALTER TABLE dbo.UPSs ADD ModeloBateriaBaseId int NULL REFERENCES dbo.ModelosBateria(Id);
IF COL_LENGTH('dbo.Mantenimientos', 'ModeloBateriaId') IS NULL
    ALTER TABLE dbo.Mantenimientos ADD ModeloBateriaId int NULL REFERENCES dbo.ModelosBateria(Id);
COMMIT;
