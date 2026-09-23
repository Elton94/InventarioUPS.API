USE [InventarioUPSDB];
GO

-- Detenga la API y respalde la base antes de ejecutar.
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Mantenimientos', N'U') IS NULL
        THROW 51000, N'No existe dbo.Mantenimientos.', 1;

    IF COL_LENGTH(N'dbo.Mantenimientos', N'IncluyeCambioBaterias') IS NULL
        THROW 51001, N'Falta el indicador histórico de cambios de baterías.', 1;

    DECLARE @Antes bigint;
    SELECT @Antes = COUNT_BIG(*) FROM dbo.Mantenimientos WITH (TABLOCKX, HOLDLOCK);

    DECLARE @Columna int = COLUMNPROPERTY(OBJECT_ID(N'dbo.Mantenimientos'), N'TipoMantenimientoId', 'ColumnId');
    DECLARE @Sql nvarchar(max) = N'';

    IF @Columna IS NOT NULL
    BEGIN
        SELECT @Sql = @Sql + N'ALTER TABLE dbo.Mantenimientos DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
        FROM sys.foreign_keys AS fk
        WHERE fk.parent_object_id = OBJECT_ID(N'dbo.Mantenimientos')
          AND EXISTS (
              SELECT 1 FROM sys.foreign_key_columns AS fc
              WHERE fc.constraint_object_id = fk.object_id AND fc.parent_column_id = @Columna
          );
        IF LEN(@Sql) > 0 EXEC sys.sp_executesql @Sql;

        SET @Sql = N'';
        SELECT @Sql = @Sql + N'DROP INDEX ' + QUOTENAME(i.name) + N' ON dbo.Mantenimientos;'
        FROM sys.indexes AS i
        WHERE i.object_id = OBJECT_ID(N'dbo.Mantenimientos')
          AND i.is_primary_key = 0 AND i.is_unique_constraint = 0
          AND (SELECT COUNT(*) FROM sys.index_columns AS ic
               WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id) = 1
          AND EXISTS (SELECT 1 FROM sys.index_columns AS ic
                      WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id
                        AND ic.column_id = @Columna);
        IF LEN(@Sql) > 0 EXEC sys.sp_executesql @Sql;

        SET @Sql = N'';
        SELECT @Sql = N'ALTER TABLE dbo.Mantenimientos DROP CONSTRAINT ' + QUOTENAME(name) + N';'
        FROM sys.default_constraints
        WHERE parent_object_id = OBJECT_ID(N'dbo.Mantenimientos') AND parent_column_id = @Columna;
        IF LEN(@Sql) > 0 EXEC sys.sp_executesql @Sql;

        EXEC(N'ALTER TABLE dbo.Mantenimientos DROP COLUMN TipoMantenimientoId;');
    END;

    IF OBJECT_ID(N'dbo.TiposMantenimiento', N'U') IS NOT NULL
        EXEC(N'DROP TABLE dbo.TiposMantenimiento;');

    DECLARE @Despues bigint;
    SELECT @Despues = COUNT_BIG(*) FROM dbo.Mantenimientos;
    IF @Antes <> @Despues
        THROW 51002, N'Cambió el número de registros. Se revierte la operación.', 1;

    COMMIT TRANSACTION;
    SELECT N'Módulo de cambios de baterías actualizado.' AS Resultado,
        @Despues AS RegistrosConservados;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
