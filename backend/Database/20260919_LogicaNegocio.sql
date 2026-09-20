USE InventarioUPSDB;
GO

/* Ejecutar una sola vez desde SQL Server Management Studio. */

IF COL_LENGTH('UPSs', 'FechaUltimoCambioBateriasBase') IS NULL
BEGIN
    ALTER TABLE UPSs ADD FechaUltimoCambioBateriasBase datetime2 NULL;

    UPDATE UPSs
    SET FechaUltimoCambioBateriasBase = FechaUltimoCambioBaterias
    WHERE FechaUltimoCambioBaterias IS NOT NULL;
END;
GO

IF COL_LENGTH('Mantenimientos', 'AgenciaIdRegistro') IS NULL
BEGIN
    ALTER TABLE Mantenimientos ADD AgenciaIdRegistro int NULL;
END;
GO

IF COL_LENGTH('Mantenimientos', 'AgenciaNombreRegistro') IS NULL
BEGIN
    ALTER TABLE Mantenimientos
    ADD AgenciaNombreRegistro nvarchar(150) NOT NULL
        CONSTRAINT DF_Mantenimientos_AgenciaNombreRegistro DEFAULT('');
END;
GO

IF COL_LENGTH('Mantenimientos', 'EstadoUPSAnteriorId') IS NULL
BEGIN
    ALTER TABLE Mantenimientos ADD EstadoUPSAnteriorId int NULL;
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
FROM Mantenimientos M
INNER JOIN UPSs U ON U.Id = M.UPSId
INNER JOIN Agencias A ON A.Id = U.AgenciaId;
GO
