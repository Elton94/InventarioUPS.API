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

-- Detener la API y respaldar la base antes de ejecutar.
-- Sustituye Id/Codigo de UPS por NumeroInventario y conserva los mantenimientos.
-- Requiere los scripts de lógica de negocio (20260919) e inventario (20260921).
-- No requiere ejecutar el antiguo script de los tres identificadores únicos.
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @TotalUPS bigint, @TotalMantenimientos bigint;
    SELECT @TotalUPS = COUNT_BIG(*) FROM dbo.UPSs WITH (TABLOCKX, HOLDLOCK);
    SELECT @TotalMantenimientos = COUNT_BIG(*) FROM dbo.Mantenimientos WITH (TABLOCKX, HOLDLOCK);

    IF COL_LENGTH('dbo.UPSs', 'Id') IS NULL
    BEGIN
        IF COL_LENGTH('dbo.UPSs', 'Codigo') IS NOT NULL
           OR COL_LENGTH('dbo.Mantenimientos', 'UPSId') IS NOT NULL
           OR COL_LENGTH('dbo.Mantenimientos', 'NumeroInventarioUPS') IS NULL
           OR NOT EXISTS (
               SELECT 1 FROM sys.key_constraints k
               JOIN sys.index_columns ic ON ic.object_id=k.parent_object_id AND ic.index_id=k.unique_index_id
               JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
               WHERE k.parent_object_id=OBJECT_ID('dbo.UPSs') AND k.type='PK'
                 AND c.name='NumeroInventario'
           )
            THROW 51000, N'El esquema no coincide con el anterior ni con el nuevo. No se aplicaron cambios.', 1;
        COMMIT;
        PRINT N'La migración a NumeroInventario ya fue aplicada.';
        RETURN;
    END;

    IF COL_LENGTH('dbo.UPSs', 'NumeroInventario') IS NULL
       OR COL_LENGTH('dbo.Mantenimientos', 'UPSId') IS NULL
        THROW 51000, N'Falta el inventario o la relación anterior. Revise los scripts previos.', 1;

    -- Diagnóstico previo. No sustituir duplicados ni valores temporales por datos inventados.
    EXEC(N'
        SELECT Id, NumeroInventario, NumeroSerie,
            COUNT(*) OVER (PARTITION BY LTRIM(RTRIM(NumeroInventario)) COLLATE Latin1_General_100_CI_AS) AS InventariosRepetidos,
            COUNT(*) OVER (PARTITION BY LTRIM(RTRIM(NumeroSerie)) COLLATE Latin1_General_100_CI_AS) AS SeriesRepetidas
        INTO #RevisionUPS FROM dbo.UPSs;
        SELECT * INTO #ProblemasUPS FROM #RevisionUPS
        WHERE NumeroInventario IS NULL OR LTRIM(RTRIM(NumeroInventario)) = N''''
           OR NumeroInventario LIKE N''INV-PEND-%''
           OR DATALENGTH(LTRIM(RTRIM(NumeroInventario))) > 200
           OR NumeroSerie IS NULL OR LTRIM(RTRIM(NumeroSerie)) = N''''
           OR DATALENGTH(LTRIM(RTRIM(NumeroSerie))) > 200
           OR InventariosRepetidos > 1 OR SeriesRepetidas > 1;
        IF EXISTS (SELECT 1 FROM #ProblemasUPS)
        BEGIN
            SELECT * FROM #ProblemasUPS ORDER BY Id;
            THROW 51001, N''Corrija inventarios temporales, vacíos, demasiado largos o duplicados y series repetidas antes de migrar.'', 1;
        END;
        IF EXISTS (
            SELECT 1 FROM dbo.Mantenimientos m
            LEFT JOIN dbo.UPSs u ON u.Id=m.UPSId WHERE u.Id IS NULL
        ) THROW 51002, N''Hay mantenimientos sin un UPS válido; no se puede migrar la relación.'', 1;
    ');

    -- Una relación externa desconocida necesita una migración explícita, no se elimina.
    IF EXISTS (
        SELECT 1 FROM sys.foreign_keys fk
        WHERE fk.referenced_object_id=OBJECT_ID('dbo.UPSs')
          AND (fk.parent_object_id<>OBJECT_ID('dbo.Mantenimientos') OR EXISTS (
              SELECT 1 FROM sys.foreign_key_columns fkc
              JOIN sys.columns c ON c.object_id=fkc.parent_object_id AND c.column_id=fkc.parent_column_id
              WHERE fkc.constraint_object_id=fk.object_id AND c.name<>'UPSId'
          ))
    ) THROW 51003, N'Existen otras relaciones con UPS. Deben migrarse antes de retirar Id.', 1;

    IF COL_LENGTH('dbo.Mantenimientos', 'NumeroInventarioUPS') IS NULL
        EXEC(N'ALTER TABLE dbo.Mantenimientos ADD NumeroInventarioUPS nvarchar(100) COLLATE Latin1_General_100_CI_AS NULL;');

    EXEC(N'
        IF EXISTS (
            SELECT 1 FROM dbo.Mantenimientos m JOIN dbo.UPSs u ON u.Id=m.UPSId
            WHERE m.NumeroInventarioUPS IS NOT NULL
              AND m.NumeroInventarioUPS COLLATE Latin1_General_100_CI_AS <> LTRIM(RTRIM(u.NumeroInventario)) COLLATE Latin1_General_100_CI_AS
        ) THROW 51004, N''La relación de inventario existente no coincide con UPSId.'', 1;
        UPDATE m SET NumeroInventarioUPS=LTRIM(RTRIM(u.NumeroInventario))
        FROM dbo.Mantenimientos m JOIN dbo.UPSs u ON u.Id=m.UPSId;
    ');

    DECLARE @SQL nvarchar(max) = N'';
    SELECT @SQL=@SQL+N'ALTER TABLE dbo.Mantenimientos DROP CONSTRAINT '+QUOTENAME(name)+N';'
    FROM sys.foreign_keys WHERE referenced_object_id=OBJECT_ID('dbo.UPSs');
    IF @SQL<>N'' EXEC sys.sp_executesql @SQL;

    -- Índices auxiliares de versiones anteriores y los índices que cambian de función.
    SET @SQL=N'';
    SELECT @SQL=@SQL+N'DROP INDEX '+QUOTENAME(name)+N' ON dbo.UPSs;'
    FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.UPSs') AND name IN (
        'UX_UPSs_CodigoUnico','UX_UPSs_NumeroInventarioUnico','UX_UPSs_NumeroSerieUnico',
        'IX_UPSs_Codigo','IX_UPSs_NumeroInventario','IX_UPSs_NumeroSerie');
    IF @SQL<>N'' EXEC sys.sp_executesql @SQL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.UPSs')
        AND name IN ('CodigoUnico','NumeroInventarioUnico','NumeroSerieUnico') AND is_computed=0)
        THROW 51005, N'Una columna auxiliar no es calculada. Revísela antes de retirarla.', 1;
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
    IF @SQL=N'' THROW 51006, N'No se encontró la clave primaria anterior de UPS.', 1;
    EXEC sys.sp_executesql @SQL;

    EXEC(N'
        UPDATE dbo.UPSs SET NumeroInventario=LTRIM(RTRIM(NumeroInventario)), NumeroSerie=LTRIM(RTRIM(NumeroSerie));
        ALTER TABLE dbo.UPSs ALTER COLUMN NumeroInventario nvarchar(100) COLLATE Latin1_General_100_CI_AS NOT NULL;
        ALTER TABLE dbo.UPSs ALTER COLUMN NumeroSerie nvarchar(100) COLLATE Latin1_General_100_CI_AS NOT NULL;
        ALTER TABLE dbo.UPSs ADD CONSTRAINT PK_UPSs PRIMARY KEY (NumeroInventario);
        CREATE UNIQUE INDEX IX_UPSs_NumeroSerie ON dbo.UPSs (NumeroSerie);
        ALTER TABLE dbo.UPSs WITH CHECK ADD CONSTRAINT CK_UPSs_NumeroInventario_Valido
            CHECK (LEN(NumeroInventario)>0 AND DATALENGTH(NumeroInventario)=DATALENGTH(LTRIM(RTRIM(NumeroInventario))));
        ALTER TABLE dbo.UPSs WITH CHECK ADD CONSTRAINT CK_UPSs_NumeroSerie_Valido
            CHECK (LEN(NumeroSerie)>0 AND DATALENGTH(NumeroSerie)=DATALENGTH(LTRIM(RTRIM(NumeroSerie))));
        ALTER TABLE dbo.Mantenimientos ALTER COLUMN NumeroInventarioUPS nvarchar(100) COLLATE Latin1_General_100_CI_AS NOT NULL;
        ALTER TABLE dbo.Mantenimientos WITH CHECK ADD CONSTRAINT FK_Mantenimientos_UPSs_NumeroInventarioUPS
            FOREIGN KEY (NumeroInventarioUPS) REFERENCES dbo.UPSs (NumeroInventario);
        CREATE INDEX IX_Mantenimientos_NumeroInventarioUPS ON dbo.Mantenimientos (NumeroInventarioUPS);
    ');

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Mantenimientos') AND name='IX_Mantenimientos_UPSId')
        EXEC(N'DROP INDEX IX_Mantenimientos_UPSId ON dbo.Mantenimientos;');
    EXEC(N'ALTER TABLE dbo.Mantenimientos DROP COLUMN UPSId; ALTER TABLE dbo.UPSs DROP COLUMN Id, Codigo;');

    IF @TotalUPS<>(SELECT COUNT_BIG(*) FROM dbo.UPSs)
       OR @TotalMantenimientos<>(SELECT COUNT_BIG(*) FROM dbo.Mantenimientos)
        THROW 51007, N'El conteo de registros cambió. Se revierte la migración.', 1;

    COMMIT TRANSACTION;
    SELECT @TotalUPS AS UPSConservados, @TotalMantenimientos AS MantenimientosConservados;
    PRINT N'NumeroInventario es la clave primaria de UPS. Codigo e Id fueron retirados.';
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
