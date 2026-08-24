using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioUPS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUPS_BateriasInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UPSs_Agencias_AgenciaId",
                table: "UPSs");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroSerie",
                table: "UPSs",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CantidadBaterias",
                table: "UPSs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimoCambioBaterias",
                table: "UPSs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UPSs_NumeroSerie",
                table: "UPSs",
                column: "NumeroSerie",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UPSs_Agencias_AgenciaId",
                table: "UPSs",
                column: "AgenciaId",
                principalTable: "Agencias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UPSs_Agencias_AgenciaId",
                table: "UPSs");

            migrationBuilder.DropIndex(
                name: "IX_UPSs_NumeroSerie",
                table: "UPSs");

            migrationBuilder.DropColumn(
                name: "CantidadBaterias",
                table: "UPSs");

            migrationBuilder.DropColumn(
                name: "FechaUltimoCambioBaterias",
                table: "UPSs");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroSerie",
                table: "UPSs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_UPSs_Agencias_AgenciaId",
                table: "UPSs",
                column: "AgenciaId",
                principalTable: "Agencias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
