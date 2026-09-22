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

/*
    Ejecutar después de los scripts 20260919 y 20260921.
    No elimina UPS ni inventa identificadores.
    Si existen datos inválidos o repetidos, muestra sus Id y revierte
    toda la operación. Corrija los valores reales y vuelva a ejecutar.
    Usa únicamente las columnas originales. Retira las columnas calculadas
    de la versión anterior si llegaron a crearse.
    Normaliza solo espacios exteriores; no inventa códigos ni números.
*/
BEGIN TRY
    BEGIN TRANSACTION;

    -- Evita inserciones concurrentes entre el diagnóstico y los índices.
    DECLARE @Cantidad bigint;
    SELECT @Cantidad = COUNT_BIG(*) FROM dbo.UPSs WITH (TABLOCKX, HOLDLOCK);

    SELECT u.Id, v.Campo, v.Valor,
           COUNT(*) OVER (
               PARTITION BY v.Campo, v.Valor COLLATE Latin1_General_100_CI_AS
           ) AS Repeticiones,
           v.LongitudMaxima
    INTO #IdentificadoresUPS
    FROM dbo.UPSs AS u
    CROSS APPLY (VALUES
        (N'Codigo', LTRIM(RTRIM(u.Codigo)), 50),
        (N'NumeroInventario', LTRIM(RTRIM(u.NumeroInventario)), 100),
        (N'NumeroSerie', LTRIM(RTRIM(u.NumeroSerie)), 100)
    ) AS v(Campo, Valor, LongitudMaxima);

    IF EXISTS (
        SELECT 1 FROM #IdentificadoresUPS
        WHERE Valor IS NULL OR Valor = N''
           OR DATALENGTH(Valor) / 2 > LongitudMaxima OR Repeticiones > 1
    )
    BEGIN
        SELECT Id, Campo, Valor, Repeticiones,
               CASE
                   WHEN Valor IS NULL OR Valor = N'' THEN N'Obligatorio'
                   WHEN DATALENGTH(Valor) / 2 > LongitudMaxima THEN N'Demasiado largo'
                   ELSE N'Duplicado'
               END AS Problema
        FROM #IdentificadoresUPS
        WHERE Valor IS NULL OR Valor = N''
           OR DATALENGTH(Valor) / 2 > LongitudMaxima OR Repeticiones > 1
        ORDER BY Campo, Valor, Id;

        THROW 51000, N'Corrija los identificadores indicados con sus valores reales y vuelva a ejecutar. No se modificó ningún UPS.', 1;
    END;

    -- Retirar únicamente la columna calculada auxiliar de la versión anterior.
    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'CodigoUnico' AND is_computed = 0
    )
        THROW 51001, N'CodigoUnico no es una columna calculada; revise su contenido antes de eliminarla.', 1;

    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'UX_UPSs_CodigoUnico'
    )
        EXEC(N'DROP INDEX UX_UPSs_CodigoUnico ON dbo.UPSs;');

    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'CodigoUnico' AND is_computed = 1
    )
        EXEC(N'ALTER TABLE dbo.UPSs DROP COLUMN CodigoUnico;');

    -- Retirar únicamente la columna calculada auxiliar de la versión anterior.
    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'NumeroInventarioUnico' AND is_computed = 0
    )
        THROW 51001, N'NumeroInventarioUnico no es una columna calculada; revise su contenido antes de eliminarla.', 1;

    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'UX_UPSs_NumeroInventarioUnico'
    )
        EXEC(N'DROP INDEX UX_UPSs_NumeroInventarioUnico ON dbo.UPSs;');

    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'NumeroInventarioUnico' AND is_computed = 1
    )
        EXEC(N'ALTER TABLE dbo.UPSs DROP COLUMN NumeroInventarioUnico;');

    -- Retirar únicamente la columna calculada auxiliar de la versión anterior.
    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'NumeroSerieUnico' AND is_computed = 0
    )
        THROW 51001, N'NumeroSerieUnico no es una columna calculada; revise su contenido antes de eliminarla.', 1;

    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'UX_UPSs_NumeroSerieUnico'
    )
        EXEC(N'DROP INDEX UX_UPSs_NumeroSerieUnico ON dbo.UPSs;');

    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'NumeroSerieUnico' AND is_computed = 1
    )
        EXEC(N'ALTER TABLE dbo.UPSs DROP COLUMN NumeroSerieUnico;');

    -- Unicidad directamente sobre Codigo.
    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'IX_UPSs_Codigo'
    )
        EXEC(N'DROP INDEX IX_UPSs_Codigo ON dbo.UPSs;');

    IF EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'CK_UPSs_Codigo_Valido'
    )
        EXEC(N'ALTER TABLE dbo.UPSs DROP CONSTRAINT CK_UPSs_Codigo_Valido;');

    UPDATE dbo.UPSs SET Codigo = LTRIM(RTRIM(Codigo))
    WHERE DATALENGTH(Codigo) <> DATALENGTH(LTRIM(RTRIM(Codigo)));

    EXEC(N'ALTER TABLE dbo.UPSs ALTER COLUMN Codigo
        nvarchar(50) COLLATE Latin1_General_100_CI_AS NOT NULL;');

    EXEC(N'ALTER TABLE dbo.UPSs WITH CHECK ADD CONSTRAINT CK_UPSs_Codigo_Valido
        CHECK (LEN(Codigo) > 0
            AND DATALENGTH(Codigo) = DATALENGTH(LTRIM(RTRIM(Codigo))));');

    EXEC(N'CREATE UNIQUE INDEX IX_UPSs_Codigo ON dbo.UPSs (Codigo);');

    -- Unicidad directamente sobre NumeroInventario.
    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'IX_UPSs_NumeroInventario'
    )
        EXEC(N'DROP INDEX IX_UPSs_NumeroInventario ON dbo.UPSs;');

    IF EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'CK_UPSs_NumeroInventario_Valido'
    )
        EXEC(N'ALTER TABLE dbo.UPSs DROP CONSTRAINT CK_UPSs_NumeroInventario_Valido;');

    UPDATE dbo.UPSs SET NumeroInventario = LTRIM(RTRIM(NumeroInventario))
    WHERE DATALENGTH(NumeroInventario) <> DATALENGTH(LTRIM(RTRIM(NumeroInventario)));

    EXEC(N'ALTER TABLE dbo.UPSs ALTER COLUMN NumeroInventario
        nvarchar(100) COLLATE Latin1_General_100_CI_AS NOT NULL;');

    EXEC(N'ALTER TABLE dbo.UPSs WITH CHECK ADD CONSTRAINT CK_UPSs_NumeroInventario_Valido
        CHECK (LEN(NumeroInventario) > 0
            AND DATALENGTH(NumeroInventario) = DATALENGTH(LTRIM(RTRIM(NumeroInventario))));');

    EXEC(N'CREATE UNIQUE INDEX IX_UPSs_NumeroInventario ON dbo.UPSs (NumeroInventario);');

    -- Unicidad directamente sobre NumeroSerie.
    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'IX_UPSs_NumeroSerie'
    )
        EXEC(N'DROP INDEX IX_UPSs_NumeroSerie ON dbo.UPSs;');

    IF EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'CK_UPSs_NumeroSerie_Valido'
    )
        EXEC(N'ALTER TABLE dbo.UPSs DROP CONSTRAINT CK_UPSs_NumeroSerie_Valido;');

    UPDATE dbo.UPSs SET NumeroSerie = LTRIM(RTRIM(NumeroSerie))
    WHERE DATALENGTH(NumeroSerie) <> DATALENGTH(LTRIM(RTRIM(NumeroSerie)));

    EXEC(N'ALTER TABLE dbo.UPSs ALTER COLUMN NumeroSerie
        nvarchar(100) COLLATE Latin1_General_100_CI_AS NOT NULL;');

    EXEC(N'ALTER TABLE dbo.UPSs WITH CHECK ADD CONSTRAINT CK_UPSs_NumeroSerie_Valido
        CHECK (LEN(NumeroSerie) > 0
            AND DATALENGTH(NumeroSerie) = DATALENGTH(LTRIM(RTRIM(NumeroSerie))));');

    EXEC(N'CREATE UNIQUE INDEX IX_UPSs_NumeroSerie ON dbo.UPSs (NumeroSerie);');

    DROP TABLE #IdentificadoresUPS;
    COMMIT TRANSACTION;

    SELECT i.name AS Indice, c.name AS Columna,
           i.is_unique AS EsUnico, i.is_disabled AS Deshabilitado
    FROM sys.indexes AS i
    INNER JOIN sys.index_columns AS ic
        ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    INNER JOIN sys.columns AS c
        ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = OBJECT_ID('dbo.UPSs')
      AND i.name IN (
          'IX_UPSs_Codigo',
          'IX_UPSs_NumeroInventario',
          'IX_UPSs_NumeroSerie'
      );
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
