USE InventarioUPSDB;
GO

/* Ejecutar una sola vez desde SQL Server Management Studio. */

IF COL_LENGTH('dbo.UPSs', 'FechaUltimoCambioBateriasBase') IS NULL
BEGIN
    ALTER TABLE dbo.UPSs
    ADD FechaUltimoCambioBateriasBase datetime2 NULL;
END;
GO

UPDATE dbo.UPSs
SET FechaUltimoCambioBateriasBase = FechaUltimoCambioBaterias
WHERE FechaUltimoCambioBaterias IS NOT NULL
  AND FechaUltimoCambioBateriasBase IS NULL;
GO

IF COL_LENGTH('dbo.Mantenimientos', 'AgenciaIdRegistro') IS NULL
BEGIN
    ALTER TABLE dbo.Mantenimientos
    ADD AgenciaIdRegistro int NULL;
END;
GO

IF COL_LENGTH('dbo.Mantenimientos', 'AgenciaNombreRegistro') IS NULL
BEGIN
    ALTER TABLE dbo.Mantenimientos
    ADD AgenciaNombreRegistro nvarchar(150) NOT NULL
        CONSTRAINT DF_Mantenimientos_AgenciaNombreRegistro DEFAULT('');
END;
GO

IF COL_LENGTH('dbo.Mantenimientos', 'EstadoUPSAnteriorId') IS NULL
BEGIN
    ALTER TABLE dbo.Mantenimientos
    ADD EstadoUPSAnteriorId int NULL;
END;
GO

UPDATE M
SET
    M.AgenciaIdRegistro = ISNULL(M.AgenciaIdRegistro, U.AgenciaId),
    M.AgenciaNombreRegistro =
        CASE
            WHEN NULLIF(LTRIM(RTRIM(M.AgenciaNombreRegistro)), '') IS NULL
                THEN A.Nombre
            ELSE M.AgenciaNombreRegistro
        END
FROM dbo.Mantenimientos M
INNER JOIN dbo.UPSs U
    ON U.Id = M.UPSId
INNER JOIN dbo.Agencias A
    ON A.Id = U.AgenciaId;
GO

/* ============================================================
   NORMALIZAR BASE HISTORICA DE BATERIAS
   Si la fecha actual coincide con un mantenimiento Completado
   que cambio baterias, esa fecha pertenece al historial y no a
   la base manual original.
   ============================================================ */

;WITH UltimoCambio AS
(
    SELECT
        M.UPSId,
        MAX(M.FechaMantenimiento) AS FechaUltimoCompletado
    FROM dbo.Mantenimientos M
    INNER JOIN dbo.EstadosMantenimiento E
        ON E.Id = M.EstadoMantenimientoId
    WHERE
        M.IncluyeCambioBaterias = 1
        AND E.Nombre = 'Completado'
    GROUP BY M.UPSId
)
UPDATE U
SET U.FechaUltimoCambioBateriasBase = NULL
FROM dbo.UPSs U
INNER JOIN UltimoCambio C
    ON C.UPSId = U.Id
WHERE
    U.FechaUltimoCambioBaterias IS NOT NULL
    AND CAST(U.FechaUltimoCambioBaterias AS date) =
        CAST(C.FechaUltimoCompletado AS date);
GO
