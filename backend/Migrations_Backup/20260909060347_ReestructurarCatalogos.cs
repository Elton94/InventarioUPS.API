using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioUPS.API.Migrations
{
    public partial class ReestructurarCatalogos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // 1. QUITAR FK EXISTENTES TEMPORALMENTE
            // ============================================================

            migrationBuilder.DropForeignKey(
                name: "FK_Mantenimientos_UPSs_UPSId",
                table: "Mantenimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Roles_RolId",
                table: "Usuarios");

            // ============================================================
            // 2. CREAR CATÁLOGOS
            // ============================================================

            migrationBuilder.CreateTable(
                name: "Departamentos",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    Nombre = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    Activo = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    FechaCreacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    FechaModificacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_Departamentos",
                        x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosUPS",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    Nombre = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false),

                    Activo = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    FechaCreacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    FechaModificacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_EstadosUPS",
                        x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosMantenimiento",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    Nombre = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false),

                    Activo = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    FechaCreacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    FechaModificacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_EstadosMantenimiento",
                        x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposAgencia",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    Nombre = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    Activo = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    FechaCreacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    FechaModificacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_TiposAgencia",
                        x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposMantenimiento",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    Nombre = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    Activo = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    FechaCreacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    FechaModificacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_TiposMantenimiento",
                        x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ciudades",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    Nombre = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    DepartamentoId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Activa = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    FechaCreacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    FechaModificacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_Ciudades",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_Ciudades_Departamentos_DepartamentoId",
                        column: x => x.DepartamentoId,
                        principalTable: "Departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ============================================================
            // 3. CREAR DATOS INICIALES DE RESPALDO
            // ============================================================

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM Departamentos
                    WHERE Nombre = 'Sin especificar'
                )
                BEGIN
                    INSERT INTO Departamentos
                    (
                        Nombre,
                        Activo,
                        FechaCreacion
                    )
                    VALUES
                    (
                        'Sin especificar',
                        1,
                        GETDATE()
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM EstadosUPS
                    WHERE Nombre = 'Sin especificar'
                )
                BEGIN
                    INSERT INTO EstadosUPS
                    (
                        Nombre,
                        Activo,
                        FechaCreacion
                    )
                    VALUES
                    (
                        'Sin especificar',
                        1,
                        GETDATE()
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM EstadosMantenimiento
                    WHERE Nombre = 'Sin especificar'
                )
                BEGIN
                    INSERT INTO EstadosMantenimiento
                    (
                        Nombre,
                        Activo,
                        FechaCreacion
                    )
                    VALUES
                    (
                        'Sin especificar',
                        1,
                        GETDATE()
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM TiposAgencia
                    WHERE Nombre = 'Sin especificar'
                )
                BEGIN
                    INSERT INTO TiposAgencia
                    (
                        Nombre,
                        Activo,
                        FechaCreacion
                    )
                    VALUES
                    (
                        'Sin especificar',
                        1,
                        GETDATE()
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM TiposMantenimiento
                    WHERE Nombre = 'Sin especificar'
                )
                BEGIN
                    INSERT INTO TiposMantenimiento
                    (
                        Nombre,
                        Activo,
                        FechaCreacion
                    )
                    VALUES
                    (
                        'Sin especificar',
                        1,
                        GETDATE()
                    );
                END
            ");

            // ============================================================
            // 4. PASAR LOS DATOS ANTIGUOS A LOS CATÁLOGOS
            // ============================================================

            // Departamentos existentes
            migrationBuilder.Sql(@"
                INSERT INTO Departamentos
                (
                    Nombre,
                    Activo,
                    FechaCreacion
                )
                SELECT DISTINCT
                    LTRIM(RTRIM(Departamento)),
                    1,
                    GETDATE()
                FROM Agencias
                WHERE Departamento IS NOT NULL
                  AND LTRIM(RTRIM(Departamento)) <> ''
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM Departamentos d
                      WHERE LOWER(LTRIM(RTRIM(d.Nombre))) =
                            LOWER(LTRIM(RTRIM(Agencias.Departamento)))
                  );
            ");

            // Ciudades existentes
            migrationBuilder.Sql(@"
                INSERT INTO Ciudades
                (
                    Nombre,
                    DepartamentoId,
                    Activa,
                    FechaCreacion
                )
                SELECT DISTINCT
                    LTRIM(RTRIM(a.Ciudad)),
                    d.Id,
                    1,
                    GETDATE()
                FROM Agencias a
                INNER JOIN Departamentos d
                    ON LOWER(LTRIM(RTRIM(d.Nombre))) =
                       LOWER(
                           CASE
                               WHEN a.Departamento IS NULL
                                    OR LTRIM(RTRIM(a.Departamento)) = ''
                               THEN 'Sin especificar'
                               ELSE LTRIM(RTRIM(a.Departamento))
                           END
                       )
                WHERE a.Ciudad IS NOT NULL
                  AND LTRIM(RTRIM(a.Ciudad)) <> ''
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM Ciudades c
                      WHERE c.DepartamentoId = d.Id
                        AND LOWER(LTRIM(RTRIM(c.Nombre))) =
                            LOWER(LTRIM(RTRIM(a.Ciudad)))
                  );
            ");

            // Tipos de agencia
            migrationBuilder.Sql(@"
                INSERT INTO TiposAgencia
                (
                    Nombre,
                    Activo,
                    FechaCreacion
                )
                SELECT DISTINCT
                    LTRIM(RTRIM(TipoAgencia)),
                    1,
                    GETDATE()
                FROM Agencias
                WHERE TipoAgencia IS NOT NULL
                  AND LTRIM(RTRIM(TipoAgencia)) <> ''
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM TiposAgencia t
                      WHERE LOWER(LTRIM(RTRIM(t.Nombre))) =
                            LOWER(LTRIM(RTRIM(Agencias.TipoAgencia)))
                  );
            ");

            // Estados de UPS
            migrationBuilder.Sql(@"
                INSERT INTO EstadosUPS
                (
                    Nombre,
                    Activo,
                    FechaCreacion
                )
                SELECT DISTINCT
                    LTRIM(RTRIM(Estado)),
                    1,
                    GETDATE()
                FROM UPSs
                WHERE Estado IS NOT NULL
                  AND LTRIM(RTRIM(Estado)) <> ''
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM EstadosUPS e
                      WHERE LOWER(LTRIM(RTRIM(e.Nombre))) =
                            LOWER(LTRIM(RTRIM(UPSs.Estado)))
                  );
            ");

            // Tipos de mantenimiento
            migrationBuilder.Sql(@"
                INSERT INTO TiposMantenimiento
                (
                    Nombre,
                    Activo,
                    FechaCreacion
                )
                SELECT DISTINCT
                    LTRIM(RTRIM(TipoMantenimiento)),
                    1,
                    GETDATE()
                FROM Mantenimientos
                WHERE TipoMantenimiento IS NOT NULL
                  AND LTRIM(RTRIM(TipoMantenimiento)) <> ''
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM TiposMantenimiento t
                      WHERE LOWER(LTRIM(RTRIM(t.Nombre))) =
                            LOWER(LTRIM(RTRIM(Mantenimientos.TipoMantenimiento)))
                  );
            ");

            // Estados de mantenimiento
            migrationBuilder.Sql(@"
                INSERT INTO EstadosMantenimiento
                (
                    Nombre,
                    Activo,
                    FechaCreacion
                )
                SELECT DISTINCT
                    LTRIM(RTRIM(Estado)),
                    1,
                    GETDATE()
                FROM Mantenimientos
                WHERE Estado IS NOT NULL
                  AND LTRIM(RTRIM(Estado)) <> ''
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM EstadosMantenimiento e
                      WHERE LOWER(LTRIM(RTRIM(e.Nombre))) =
                            LOWER(LTRIM(RTRIM(Mantenimientos.Estado)))
                  );
            ");

            // ============================================================
            // 5. AGREGAR NUEVAS COLUMNAS COMO NULLABLES TEMPORALMENTE
            // ============================================================

            migrationBuilder.AddColumn<int>(
                name: "EstadoUPSId",
                table: "UPSs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoMantenimientoId",
                table: "Mantenimientos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoMantenimientoId",
                table: "Mantenimientos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CiudadId",
                table: "Agencias",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoAgenciaId",
                table: "Agencias",
                type: "int",
                nullable: true);

            // ============================================================
            // 6. CONVERTIR LOS DATOS ANTIGUOS A IDs
            // ============================================================

            // Agencia → Ciudad
            migrationBuilder.Sql(@"
                UPDATE a
                SET CiudadId = c.Id
                FROM Agencias a
                INNER JOIN Departamentos d
                    ON LOWER(LTRIM(RTRIM(d.Nombre))) =
                       LOWER(
                           CASE
                               WHEN a.Departamento IS NULL
                                    OR LTRIM(RTRIM(a.Departamento)) = ''
                               THEN 'Sin especificar'
                               ELSE LTRIM(RTRIM(a.Departamento))
                           END
                       )
                INNER JOIN Ciudades c
                    ON c.DepartamentoId = d.Id
                   AND LOWER(LTRIM(RTRIM(c.Nombre))) =
                       LOWER(
                           CASE
                               WHEN a.Ciudad IS NULL
                                    OR LTRIM(RTRIM(a.Ciudad)) = ''
                               THEN 'Sin especificar'
                               ELSE LTRIM(RTRIM(a.Ciudad))
                           END
                       );
            ");

            // Agencia → Tipo
            migrationBuilder.Sql(@"
                UPDATE a
                SET TipoAgenciaId = t.Id
                FROM Agencias a
                INNER JOIN TiposAgencia t
                    ON LOWER(LTRIM(RTRIM(t.Nombre))) =
                       LOWER(
                           CASE
                               WHEN a.TipoAgencia IS NULL
                                    OR LTRIM(RTRIM(a.TipoAgencia)) = ''
                               THEN 'Sin especificar'
                               ELSE LTRIM(RTRIM(a.TipoAgencia))
                           END
                       );
            ");

            // UPS → Estado
            migrationBuilder.Sql(@"
                UPDATE u
                SET EstadoUPSId = e.Id
                FROM UPSs u
                INNER JOIN EstadosUPS e
                    ON LOWER(LTRIM(RTRIM(e.Nombre))) =
                       LOWER(
                           CASE
                               WHEN u.Estado IS NULL
                                    OR LTRIM(RTRIM(u.Estado)) = ''
                               THEN 'Sin especificar'
                               ELSE LTRIM(RTRIM(u.Estado))
                           END
                       );
            ");

            // Mantenimiento → Tipo
            migrationBuilder.Sql(@"
                UPDATE m
                SET TipoMantenimientoId = t.Id
                FROM Mantenimientos m
                INNER JOIN TiposMantenimiento t
                    ON LOWER(LTRIM(RTRIM(t.Nombre))) =
                       LOWER(
                           CASE
                               WHEN m.TipoMantenimiento IS NULL
                                    OR LTRIM(RTRIM(m.TipoMantenimiento)) = ''
                               THEN 'Sin especificar'
                               ELSE LTRIM(RTRIM(m.TipoMantenimiento))
                           END
                       );
            ");

            // Mantenimiento → Estado
            migrationBuilder.Sql(@"
                UPDATE m
                SET EstadoMantenimientoId = e.Id
                FROM Mantenimientos m
                INNER JOIN EstadosMantenimiento e
                    ON LOWER(LTRIM(RTRIM(e.Nombre))) =
                       LOWER(
                           CASE
                               WHEN m.Estado IS NULL
                                    OR LTRIM(RTRIM(m.Estado)) = ''
                               THEN 'Sin especificar'
                               ELSE LTRIM(RTRIM(m.Estado))
                           END
                       );
            ");

            // ============================================================
            // 7. ASEGURAR QUE TODOS LOS REGISTROS TENGAN RELACIÓN
            // ============================================================

            migrationBuilder.Sql(@"
                UPDATE Agencias
                SET CiudadId =
                (
                    SELECT TOP 1 Id
                    FROM Ciudades
                    WHERE Nombre = 'Sin especificar'
                )
                WHERE CiudadId IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE Agencias
                SET TipoAgenciaId =
                (
                    SELECT TOP 1 Id
                    FROM TiposAgencia
                    WHERE Nombre = 'Sin especificar'
                )
                WHERE TipoAgenciaId IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE UPSs
                SET EstadoUPSId =
                (
                    SELECT TOP 1 Id
                    FROM EstadosUPS
                    WHERE Nombre = 'Sin especificar'
                )
                WHERE EstadoUPSId IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE Mantenimientos
                SET TipoMantenimientoId =
                (
                    SELECT TOP 1 Id
                    FROM TiposMantenimiento
                    WHERE Nombre = 'Sin especificar'
                )
                WHERE TipoMantenimientoId IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE Mantenimientos
                SET EstadoMantenimientoId =
                (
                    SELECT TOP 1 Id
                    FROM EstadosMantenimiento
                    WHERE Nombre = 'Sin especificar'
                )
                WHERE EstadoMantenimientoId IS NULL;
            ");

            // ============================================================
            // 8. CONVERTIR LOS IDs A OBLIGATORIOS
            // ============================================================

            migrationBuilder.AlterColumn<int>(
                name: "EstadoUPSId",
                table: "UPSs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EstadoMantenimientoId",
                table: "Mantenimientos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TipoMantenimientoId",
                table: "Mantenimientos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CiudadId",
                table: "Agencias",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TipoAgenciaId",
                table: "Agencias",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // ============================================================
            // 9. ELIMINAR CAMPOS ANTIGUOS
            // ============================================================

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "UPSs");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Mantenimientos");

            migrationBuilder.DropColumn(
                name: "TipoMantenimiento",
                table: "Mantenimientos");

            migrationBuilder.DropColumn(
                name: "Ciudad",
                table: "Agencias");

            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "Agencias");

            migrationBuilder.DropColumn(
                name: "TipoAgencia",
                table: "Agencias");

            // ============================================================
            // 10. AJUSTAR CAMPOS DE TEXTO
            // ============================================================

            migrationBuilder.AlterColumn<string>(
                name: "NombreUsuario",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NombreCompleto",
                table: "Usuarios",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Usuarios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroSerie",
                table: "UPSs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Modelo",
                table: "UPSs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Marca",
                table: "UPSs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "UPSs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Roles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Roles",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TecnicoProveedor",
                table: "Mantenimientos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Mantenimientos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Mantenimientos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Agencias",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Agencias",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Agencias",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // ============================================================
            // 11. ÍNDICES
            // ============================================================

            migrationBuilder.CreateIndex(
                name: "IX_UPSs_EstadoUPSId",
                table: "UPSs",
                column: "EstadoUPSId");

            migrationBuilder.CreateIndex(
                name: "IX_Mantenimientos_EstadoMantenimientoId",
                table: "Mantenimientos",
                column: "EstadoMantenimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Mantenimientos_TipoMantenimientoId",
                table: "Mantenimientos",
                column: "TipoMantenimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencias_CiudadId",
                table: "Agencias",
                column: "CiudadId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencias_TipoAgenciaId",
                table: "Agencias",
                column: "TipoAgenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ciudades_DepartamentoId_Nombre",
                table: "Ciudades",
                columns: new[] { "DepartamentoId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departamentos_Nombre",
                table: "Departamentos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadosMantenimiento_Nombre",
                table: "EstadosMantenimiento",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadosUPS_Nombre",
                table: "EstadosUPS",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposAgencia_Nombre",
                table: "TiposAgencia",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposMantenimiento_Nombre",
                table: "TiposMantenimiento",
                column: "Nombre",
                unique: true);

            // ============================================================
            // 12. FOREIGN KEYS
            // ============================================================

            migrationBuilder.AddForeignKey(
                name: "FK_Agencias_Ciudades_CiudadId",
                table: "Agencias",
                column: "CiudadId",
                principalTable: "Ciudades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Agencias_TiposAgencia_TipoAgenciaId",
                table: "Agencias",
                column: "TipoAgenciaId",
                principalTable: "TiposAgencia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mantenimientos_EstadosMantenimiento_EstadoMantenimientoId",
                table: "Mantenimientos",
                column: "EstadoMantenimientoId",
                principalTable: "EstadosMantenimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mantenimientos_TiposMantenimiento_TipoMantenimientoId",
                table: "Mantenimientos",
                column: "TipoMantenimientoId",
                principalTable: "TiposMantenimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mantenimientos_UPSs_UPSId",
                table: "Mantenimientos",
                column: "UPSId",
                principalTable: "UPSs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UPSs_EstadosUPS_EstadoUPSId",
                table: "UPSs",
                column: "EstadoUPSId",
                principalTable: "EstadosUPS",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Roles_RolId",
                table: "Usuarios",
                column: "RolId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "Esta migración de reestructuración no debe revertirse automáticamente porque realiza una migración de datos.");
        }
    }
}