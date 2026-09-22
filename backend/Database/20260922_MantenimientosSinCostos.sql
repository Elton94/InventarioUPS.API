USE [InventarioUPSDB];
GO

-- Detenga la API y respalde la base antes de ejecutar: los importes se eliminan.
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Mantenimientos', N'U') IS NULL
        THROW 51000, N'No existe dbo.Mantenimientos en esta base.', 1;

    DECLARE @Antes bigint;
    SELECT @Antes = COUNT_BIG(*) FROM dbo.Mantenimientos WITH (TABLOCKX, HOLDLOCK);

    DECLARE @Sql nvarchar(max) = N'';
    SELECT @Sql = @Sql + N'ALTER TABLE dbo.Mantenimientos DROP CONSTRAINT '
        + QUOTENAME(dc.name) + N';'
    FROM sys.default_constraints AS dc
    INNER JOIN sys.columns AS c
        ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Mantenimientos')
      AND c.name IN (N'Costo', N'CostoBaterias');

    IF LEN(@Sql) > 0
        EXEC sys.sp_executesql @Sql;

    IF COL_LENGTH(N'dbo.Mantenimientos', N'Costo') IS NOT NULL
        EXEC(N'ALTER TABLE dbo.Mantenimientos DROP COLUMN Costo;');

    IF COL_LENGTH(N'dbo.Mantenimientos', N'CostoBaterias') IS NOT NULL
        EXEC(N'ALTER TABLE dbo.Mantenimientos DROP COLUMN CostoBaterias;');

    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.Mantenimientos')
          AND name IN (N'Costo', N'CostoBaterias')
    )
        THROW 51001, N'No se eliminaron todas las columnas de costos.', 1;

    DECLARE @Despues bigint;
    SELECT @Despues = COUNT_BIG(*) FROM dbo.Mantenimientos;
    IF @Antes <> @Despues
        THROW 51002, N'Cambió el número de mantenimientos. Se revierte la operación.', 1;

    COMMIT TRANSACTION;
    SELECT N'Costos eliminados correctamente.' AS Resultado,
        @Despues AS MantenimientosConservados;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
