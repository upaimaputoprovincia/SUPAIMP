using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarUnidadesOperacionais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnidadeOperacionalId",
                table: "Postos",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FuncaoOperacionalId",
                table: "LotacoesFuncionarios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnidadeOperacionalId",
                table: "LotacoesFuncionarios",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SectorId",
                table: "Equipas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "UnidadeOperacionalId",
                table: "Equipas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UnidadesOperacionais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    SeccaoId = table.Column<int>(type: "int", nullable: false),
                    UnidadePaiId = table.Column<int>(type: "int", nullable: true),
                    Descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesOperacionais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnidadesOperacionais_Seccoes_SeccaoId",
                        column: x => x.SeccaoId,
                        principalTable: "Seccoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnidadesOperacionais_UnidadesOperacionais_UnidadePaiId",
                        column: x => x.UnidadePaiId,
                        principalTable: "UnidadesOperacionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Postos_UnidadeOperacionalId",
                table: "Postos",
                column: "UnidadeOperacionalId");

            migrationBuilder.CreateIndex(
                name: "IX_LotacoesFuncionarios_UnidadeOperacionalId",
                table: "LotacoesFuncionarios",
                column: "UnidadeOperacionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipas_UnidadeOperacionalId",
                table: "Equipas",
                column: "UnidadeOperacionalId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesOperacionais_SeccaoId",
                table: "UnidadesOperacionais",
                column: "SeccaoId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesOperacionais_UnidadePaiId",
                table: "UnidadesOperacionais",
                column: "UnidadePaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipas_UnidadesOperacionais_UnidadeOperacionalId",
                table: "Equipas",
                column: "UnidadeOperacionalId",
                principalTable: "UnidadesOperacionais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LotacoesFuncionarios_UnidadesOperacionais_UnidadeOperacional~",
                table: "LotacoesFuncionarios",
                column: "UnidadeOperacionalId",
                principalTable: "UnidadesOperacionais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Postos_UnidadesOperacionais_UnidadeOperacionalId",
                table: "Postos",
                column: "UnidadeOperacionalId",
                principalTable: "UnidadesOperacionais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipas_UnidadesOperacionais_UnidadeOperacionalId",
                table: "Equipas");

            migrationBuilder.DropForeignKey(
                name: "FK_LotacoesFuncionarios_UnidadesOperacionais_UnidadeOperacional~",
                table: "LotacoesFuncionarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Postos_UnidadesOperacionais_UnidadeOperacionalId",
                table: "Postos");

            migrationBuilder.DropTable(
                name: "UnidadesOperacionais");

            migrationBuilder.DropIndex(
                name: "IX_Postos_UnidadeOperacionalId",
                table: "Postos");

            migrationBuilder.DropIndex(
                name: "IX_LotacoesFuncionarios_UnidadeOperacionalId",
                table: "LotacoesFuncionarios");

            migrationBuilder.DropIndex(
                name: "IX_Equipas_UnidadeOperacionalId",
                table: "Equipas");

            migrationBuilder.DropColumn(
                name: "UnidadeOperacionalId",
                table: "Postos");

            migrationBuilder.DropColumn(
                name: "UnidadeOperacionalId",
                table: "LotacoesFuncionarios");

            migrationBuilder.DropColumn(
                name: "UnidadeOperacionalId",
                table: "Equipas");

            migrationBuilder.AlterColumn<int>(
                name: "FuncaoOperacionalId",
                table: "LotacoesFuncionarios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SectorId",
                table: "Equipas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
