using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEquipaIdEscalas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EquipaId",
                table: "Escalas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FuncaoOperacionalId",
                table: "Escalas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeccaoId",
                table: "Escalas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnidadeOperacionalId",
                table: "Escalas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_EquipaId",
                table: "Escalas",
                column: "EquipaId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_FuncaoOperacionalId",
                table: "Escalas",
                column: "FuncaoOperacionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_SeccaoId",
                table: "Escalas",
                column: "SeccaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_UnidadeOperacionalId",
                table: "Escalas",
                column: "UnidadeOperacionalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Escalas_Equipas_EquipaId",
                table: "Escalas",
                column: "EquipaId",
                principalTable: "Equipas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Escalas_FuncoesOperacionais_FuncaoOperacionalId",
                table: "Escalas",
                column: "FuncaoOperacionalId",
                principalTable: "FuncoesOperacionais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Escalas_Seccoes_SeccaoId",
                table: "Escalas",
                column: "SeccaoId",
                principalTable: "Seccoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Escalas_UnidadesOperacionais_UnidadeOperacionalId",
                table: "Escalas",
                column: "UnidadeOperacionalId",
                principalTable: "UnidadesOperacionais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Escalas_Equipas_EquipaId",
                table: "Escalas");

            migrationBuilder.DropForeignKey(
                name: "FK_Escalas_FuncoesOperacionais_FuncaoOperacionalId",
                table: "Escalas");

            migrationBuilder.DropForeignKey(
                name: "FK_Escalas_Seccoes_SeccaoId",
                table: "Escalas");

            migrationBuilder.DropForeignKey(
                name: "FK_Escalas_UnidadesOperacionais_UnidadeOperacionalId",
                table: "Escalas");

            migrationBuilder.DropIndex(
                name: "IX_Escalas_EquipaId",
                table: "Escalas");

            migrationBuilder.DropIndex(
                name: "IX_Escalas_FuncaoOperacionalId",
                table: "Escalas");

            migrationBuilder.DropIndex(
                name: "IX_Escalas_SeccaoId",
                table: "Escalas");

            migrationBuilder.DropIndex(
                name: "IX_Escalas_UnidadeOperacionalId",
                table: "Escalas");

            migrationBuilder.DropColumn(
                name: "EquipaId",
                table: "Escalas");

            migrationBuilder.DropColumn(
                name: "FuncaoOperacionalId",
                table: "Escalas");

            migrationBuilder.DropColumn(
                name: "SeccaoId",
                table: "Escalas");

            migrationBuilder.DropColumn(
                name: "UnidadeOperacionalId",
                table: "Escalas");
        }
    }
}
