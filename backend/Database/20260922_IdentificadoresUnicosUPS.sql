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
    Las columnas calculadas son auxiliares de SQL Server, no campos de la API.
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

    IF COL_LENGTH('dbo.UPSs', 'CodigoUnico') IS NULL
        EXEC(N'ALTER TABLE dbo.UPSs ADD CodigoUnico AS
            (CONVERT(nvarchar(50), LTRIM(RTRIM(Codigo))) COLLATE Latin1_General_100_CI_AS) PERSISTED;');

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'CK_UPSs_Codigo_Valido'
    )
        EXEC(N'ALTER TABLE dbo.UPSs WITH CHECK ADD CONSTRAINT CK_UPSs_Codigo_Valido
            CHECK (Codigo IS NOT NULL AND LEN(LTRIM(RTRIM(Codigo))) > 0
                AND DATALENGTH(LTRIM(RTRIM(Codigo))) / 2 <= 50);');

    -- Rehabilita y verifica también una restricción existente.
    ALTER TABLE dbo.UPSs WITH CHECK CHECK CONSTRAINT CK_UPSs_Codigo_Valido;

    -- Recrear en la misma transacción garantiza que no quede un índice
    -- previo no único, filtrado o deshabilitado con este nombre.
    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'UX_UPSs_CodigoUnico'
    )
        DROP INDEX UX_UPSs_CodigoUnico ON dbo.UPSs;

    EXEC(N'CREATE UNIQUE INDEX UX_UPSs_CodigoUnico ON dbo.UPSs (CodigoUnico);');

    IF COL_LENGTH('dbo.UPSs', 'NumeroInventarioUnico') IS NULL
        EXEC(N'ALTER TABLE dbo.UPSs ADD NumeroInventarioUnico AS
            (CONVERT(nvarchar(100), LTRIM(RTRIM(NumeroInventario))) COLLATE Latin1_General_100_CI_AS) PERSISTED;');

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'CK_UPSs_NumeroInventario_Valido'
    )
        EXEC(N'ALTER TABLE dbo.UPSs WITH CHECK ADD CONSTRAINT CK_UPSs_NumeroInventario_Valido
            CHECK (NumeroInventario IS NOT NULL AND LEN(LTRIM(RTRIM(NumeroInventario))) > 0
                AND DATALENGTH(LTRIM(RTRIM(NumeroInventario))) / 2 <= 100);');

    -- Rehabilita y verifica también una restricción existente.
    ALTER TABLE dbo.UPSs WITH CHECK CHECK CONSTRAINT CK_UPSs_NumeroInventario_Valido;

    -- Recrear en la misma transacción garantiza que no quede un índice
    -- previo no único, filtrado o deshabilitado con este nombre.
    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'UX_UPSs_NumeroInventarioUnico'
    )
        DROP INDEX UX_UPSs_NumeroInventarioUnico ON dbo.UPSs;

    EXEC(N'CREATE UNIQUE INDEX UX_UPSs_NumeroInventarioUnico ON dbo.UPSs (NumeroInventarioUnico);');

    IF COL_LENGTH('dbo.UPSs', 'NumeroSerieUnico') IS NULL
        EXEC(N'ALTER TABLE dbo.UPSs ADD NumeroSerieUnico AS
            (CONVERT(nvarchar(100), LTRIM(RTRIM(NumeroSerie))) COLLATE Latin1_General_100_CI_AS) PERSISTED;');

    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'CK_UPSs_NumeroSerie_Valido'
    )
        EXEC(N'ALTER TABLE dbo.UPSs WITH CHECK ADD CONSTRAINT CK_UPSs_NumeroSerie_Valido
            CHECK (NumeroSerie IS NOT NULL AND LEN(LTRIM(RTRIM(NumeroSerie))) > 0
                AND DATALENGTH(LTRIM(RTRIM(NumeroSerie))) / 2 <= 100);');

    -- Rehabilita y verifica también una restricción existente.
    ALTER TABLE dbo.UPSs WITH CHECK CHECK CONSTRAINT CK_UPSs_NumeroSerie_Valido;

    -- Recrear en la misma transacción garantiza que no quede un índice
    -- previo no único, filtrado o deshabilitado con este nombre.
    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.UPSs')
          AND name = 'UX_UPSs_NumeroSerieUnico'
    )
        DROP INDEX UX_UPSs_NumeroSerieUnico ON dbo.UPSs;

    EXEC(N'CREATE UNIQUE INDEX UX_UPSs_NumeroSerieUnico ON dbo.UPSs (NumeroSerieUnico);');

    DROP TABLE #IdentificadoresUPS;
    COMMIT TRANSACTION;

    SELECT name AS Indice, is_unique AS EsUnico, is_disabled AS Deshabilitado
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.UPSs')
      AND name IN (
          'UX_UPSs_CodigoUnico',
          'UX_UPSs_NumeroInventarioUnico',
          'UX_UPSs_NumeroSerieUnico'
      );
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
