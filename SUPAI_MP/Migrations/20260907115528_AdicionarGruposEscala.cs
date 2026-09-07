using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarGruposEscala : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SeccaoId",
                table: "LotacoesFuncionarios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "GruposEscala",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrdemRotacao = table.Column<int>(type: "int", nullable: false),
                    UnidadeOperacionalId = table.Column<int>(type: "int", nullable: false),
                    EquipaId = table.Column<int>(type: "int", nullable: false),
                    TipoTurnoId = table.Column<int>(type: "int", nullable: false),
                    DataReferencia = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Observacao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCadastro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposEscala", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GruposEscala_Equipas_EquipaId",
                        column: x => x.EquipaId,
                        principalTable: "Equipas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GruposEscala_TiposTurno_TipoTurnoId",
                        column: x => x.TipoTurnoId,
                        principalTable: "TiposTurno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GruposEscala_UnidadesOperacionais_UnidadeOperacionalId",
                        column: x => x.UnidadeOperacionalId,
                        principalTable: "UnidadesOperacionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GruposEscala_EquipaId",
                table: "GruposEscala",
                column: "EquipaId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposEscala_TipoTurnoId",
                table: "GruposEscala",
                column: "TipoTurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposEscala_UnidadeOperacionalId_OrdemRotacao",
                table: "GruposEscala",
                columns: new[] { "UnidadeOperacionalId", "OrdemRotacao" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GruposEscala");

            migrationBuilder.AlterColumn<int>(
                name: "SeccaoId",
                table: "LotacoesFuncionarios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
