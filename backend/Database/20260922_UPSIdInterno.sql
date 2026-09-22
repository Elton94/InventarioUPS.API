USE InventarioUPSDB;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

-- Versión vigente: Id IDENTITY como PK, inventario y serie únicos, sin Codigo.
-- Detener la API y respaldar la base antes de ejecutar.
-- Admite el esquema original y el esquema intermedio con inventario como PK.
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @TotalUPS bigint, @TotalMantenimientos bigint;
    SELECT @TotalUPS=COUNT_BIG(*) FROM dbo.UPSs WITH (TABLOCKX,HOLDLOCK);
    SELECT @TotalMantenimientos=COUNT_BIG(*) FROM dbo.Mantenimientos WITH (TABLOCKX,HOLDLOCK);

    IF COL_LENGTH('dbo.UPSs','NumeroInventario') IS NULL
        THROW 51000, N'Falta NumeroInventario. Aplique primero 20260921_NumeroInventarioUPS.sql.', 1;

    -- Permite identificadores temporales únicos: podrán corregirse desde la app.
    SELECT u.NumeroInventario, u.NumeroSerie,
        COUNT(*) OVER (PARTITION BY LTRIM(RTRIM(u.NumeroInventario)) COLLATE Latin1_General_100_CI_AS) AS InventariosRepetidos,
        COUNT(*) OVER (PARTITION BY LTRIM(RTRIM(u.NumeroSerie)) COLLATE Latin1_General_100_CI_AS) AS SeriesRepetidas
    INTO #RevisionUPS FROM dbo.UPSs u;
    IF EXISTS (SELECT 1 FROM #RevisionUPS WHERE
        NumeroInventario IS NULL OR LTRIM(RTRIM(NumeroInventario))=N''
        OR NumeroSerie IS NULL OR LTRIM(RTRIM(NumeroSerie))=N''
        OR DATALENGTH(LTRIM(RTRIM(NumeroInventario)))>200
        OR DATALENGTH(LTRIM(RTRIM(NumeroSerie)))>200
        OR InventariosRepetidos>1 OR SeriesRepetidas>1)
    BEGIN
        SELECT * FROM #RevisionUPS WHERE
            NumeroInventario IS NULL OR LTRIM(RTRIM(NumeroInventario))=N''
            OR NumeroSerie IS NULL OR LTRIM(RTRIM(NumeroSerie))=N''
            OR DATALENGTH(LTRIM(RTRIM(NumeroInventario)))>200
            OR DATALENGTH(LTRIM(RTRIM(NumeroSerie)))>200
            OR InventariosRepetidos>1 OR SeriesRepetidas>1;
        THROW 51001, N'Corrija los identificadores vacíos, largos o repetidos y vuelva a ejecutar. No se modificaron datos.', 1;
    END;
    DROP TABLE #RevisionUPS;

    DECLARE @TeniaId bit=CASE WHEN COL_LENGTH('dbo.UPSs','Id') IS NULL THEN 0 ELSE 1 END;
    DECLARE @TeniaUPSId bit=CASE WHEN COL_LENGTH('dbo.Mantenimientos','UPSId') IS NULL THEN 0 ELSE 1 END;
    DECLARE @TieneInventarioFK bit=CASE WHEN COL_LENGTH('dbo.Mantenimientos','NumeroInventarioUPS') IS NULL THEN 0 ELSE 1 END;
    IF (@TeniaId=0 AND @TeniaUPSId=1) OR (@TeniaUPSId=0 AND @TieneInventarioFK=0)
        THROW 51002, N'El esquema de relaciones no es uno de los soportados. No se aplicaron cambios.', 1;

    -- Solo se retiran las relaciones de mantenimiento que esta migración convierte.
    IF EXISTS (
        SELECT 1 FROM sys.foreign_keys fk
        WHERE fk.referenced_object_id=OBJECT_ID('dbo.UPSs')
        AND (fk.parent_object_id<>OBJECT_ID('dbo.Mantenimientos') OR EXISTS (
            SELECT 1 FROM sys.foreign_key_columns fkc
            JOIN sys.columns c ON c.object_id=fkc.parent_object_id AND c.column_id=fkc.parent_column_id
            WHERE fkc.constraint_object_id=fk.object_id AND c.name NOT IN ('UPSId','NumeroInventarioUPS')
        ))
    ) THROW 51003, N'Hay otras relaciones hacia UPS que requieren una migración explícita.', 1;

    -- Si se retiró Id en la versión intermedia, SQL Server asigna uno a cada UPS.
    -- Si todavía existía, sus valores se conservan.
    IF @TeniaId=0
        EXEC(N'ALTER TABLE dbo.UPSs ADD Id int IDENTITY(1,1) NOT NULL;');
    IF COLUMNPROPERTY(OBJECT_ID('dbo.UPSs'),'Id','IsIdentity')<>1
        THROW 51004, N'El Id existente no es IDENTITY. Revise su definición antes de continuar.', 1;
    IF @TeniaUPSId=0
        EXEC(N'ALTER TABLE dbo.Mantenimientos ADD UPSId int NULL;');

    IF @TeniaUPSId=1
        EXEC(N'IF EXISTS (SELECT 1 FROM dbo.Mantenimientos m LEFT JOIN dbo.UPSs u ON u.Id=m.UPSId WHERE u.Id IS NULL)
            THROW 51005, N''Hay mantenimientos sin un UPS válido.'', 1;');

    IF @TieneInventarioFK=1
    BEGIN
        EXEC(N'IF EXISTS (
            SELECT 1 FROM dbo.Mantenimientos m LEFT JOIN dbo.UPSs u
              ON LTRIM(RTRIM(m.NumeroInventarioUPS)) COLLATE Latin1_General_100_CI_AS
               = LTRIM(RTRIM(u.NumeroInventario)) COLLATE Latin1_General_100_CI_AS
            WHERE u.Id IS NULL OR (m.UPSId IS NOT NULL AND m.UPSId<>u.Id)
        ) THROW 51006, N''La relación por inventario no coincide con un UPS válido.'', 1;
        UPDATE m SET UPSId=u.Id FROM dbo.Mantenimientos m JOIN dbo.UPSs u
          ON LTRIM(RTRIM(m.NumeroInventarioUPS)) COLLATE Latin1_General_100_CI_AS
           = LTRIM(RTRIM(u.NumeroInventario)) COLLATE Latin1_General_100_CI_AS;');
    END;

    DECLARE @SQL nvarchar(max)=N'';
    SELECT @SQL=@SQL+N'ALTER TABLE dbo.Mantenimientos DROP CONSTRAINT '+QUOTENAME(name)+N';'
    FROM sys.foreign_keys WHERE referenced_object_id=OBJECT_ID('dbo.UPSs');
    IF @SQL<>N'' EXEC sys.sp_executesql @SQL;

    SET @SQL=N'';
    SELECT @SQL=@SQL+N'DROP INDEX '+QUOTENAME(name)+N' ON dbo.UPSs;'
    FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.UPSs') AND name IN (
        'IX_UPSs_Codigo','IX_UPSs_NumeroInventario','IX_UPSs_NumeroSerie',
        'UX_UPSs_CodigoUnico','UX_UPSs_NumeroInventarioUnico','UX_UPSs_NumeroSerieUnico');
    IF @SQL<>N'' EXEC sys.sp_executesql @SQL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.UPSs')
        AND name IN ('CodigoUnico','NumeroInventarioUnico','NumeroSerieUnico') AND is_computed=0)
        THROW 51007, N'Una columna auxiliar no es calculada. Revise sus datos antes de retirarla.', 1;
    SET @SQL=N'';
    SELECT @SQL=@SQL+N'ALTER TABLE dbo.UPSs DROP COLUMN '+QUOTENAME(name)+N';'
    FROM sys.columns WHERE object_id=OBJECT_ID('dbo.UPSs') AND is_computed=1
        AND name IN ('CodigoUnico','NumeroInventarioUnico','NumeroSerieUnico');
    IF @SQL<>N'' EXEC sys.sp_executesql @SQL;

    SET @SQL=N'';
    SELECT @SQL=@SQL+N'ALTER TABLE dbo.UPSs DROP CONSTRAINT '+QUOTENAME(name)+N';'
    FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID('dbo.UPSs')
        AND name IN ('CK_UPSs_Codigo_Valido','CK_UPSs_NumeroInventario_Valido','CK_UPSs_NumeroSerie_Valido');
    IF @SQL<>N'' EXEC sys.sp_executesql @SQL;

    SET @SQL=N'';
    SELECT @SQL=N'ALTER TABLE dbo.UPSs DROP CONSTRAINT '+QUOTENAME(name)+N';'
    FROM sys.key_constraints WHERE parent_object_id=OBJECT_ID('dbo.UPSs') AND type='PK';
    IF @SQL=N'' THROW 51008, N'No se encontró la clave primaria anterior de UPS.', 1;
    EXEC sys.sp_executesql @SQL;

    EXEC(N'
        UPDATE dbo.UPSs SET NumeroInventario=LTRIM(RTRIM(NumeroInventario)), NumeroSerie=LTRIM(RTRIM(NumeroSerie));
        ALTER TABLE dbo.UPSs ALTER COLUMN NumeroInventario nvarchar(100) COLLATE Latin1_General_100_CI_AS NOT NULL;
        ALTER TABLE dbo.UPSs ALTER COLUMN NumeroSerie nvarchar(100) COLLATE Latin1_General_100_CI_AS NOT NULL;
        ALTER TABLE dbo.UPSs ADD CONSTRAINT PK_UPSs PRIMARY KEY (Id);
        CREATE UNIQUE INDEX IX_UPSs_NumeroInventario ON dbo.UPSs (NumeroInventario);
        CREATE UNIQUE INDEX IX_UPSs_NumeroSerie ON dbo.UPSs (NumeroSerie);
        ALTER TABLE dbo.UPSs WITH CHECK ADD CONSTRAINT CK_UPSs_NumeroInventario_Valido
            CHECK (LEN(NumeroInventario)>0 AND DATALENGTH(NumeroInventario)=DATALENGTH(LTRIM(RTRIM(NumeroInventario))));
        ALTER TABLE dbo.UPSs WITH CHECK ADD CONSTRAINT CK_UPSs_NumeroSerie_Valido
            CHECK (LEN(NumeroSerie)>0 AND DATALENGTH(NumeroSerie)=DATALENGTH(LTRIM(RTRIM(NumeroSerie))));
        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N''dbo.Mantenimientos'') AND name=N''UPSId'' AND is_nullable=1)
            ALTER TABLE dbo.Mantenimientos ALTER COLUMN UPSId int NOT NULL;
        ALTER TABLE dbo.Mantenimientos WITH CHECK ADD CONSTRAINT FK_Mantenimientos_UPSs_UPSId
            FOREIGN KEY (UPSId) REFERENCES dbo.UPSs (Id);
    ');
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Mantenimientos') AND name='IX_Mantenimientos_UPSId')
        EXEC(N'CREATE INDEX IX_Mantenimientos_UPSId ON dbo.Mantenimientos (UPSId);');

    IF @TieneInventarioFK=1
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Mantenimientos') AND name='IX_Mantenimientos_NumeroInventarioUPS')
            EXEC(N'DROP INDEX IX_Mantenimientos_NumeroInventarioUPS ON dbo.Mantenimientos;');
        EXEC(N'ALTER TABLE dbo.Mantenimientos DROP COLUMN NumeroInventarioUPS;');
    END;
    IF COL_LENGTH('dbo.UPSs','Codigo') IS NOT NULL
        EXEC(N'ALTER TABLE dbo.UPSs DROP COLUMN Codigo;');

    IF @TotalUPS<>(SELECT COUNT_BIG(*) FROM dbo.UPSs) OR @TotalMantenimientos<>(SELECT COUNT_BIG(*) FROM dbo.Mantenimientos)
        THROW 51009, N'Cambió el número de registros. Se revierte la migración.', 1;
    COMMIT;
    SELECT @TotalUPS AS UPSConservados, @TotalMantenimientos AS MantenimientosConservados;
    PRINT N'Id es la clave primaria interna. Inventario y serie son únicos y editables. Codigo fue retirado.';
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK;
    THROW;
END CATCH;
