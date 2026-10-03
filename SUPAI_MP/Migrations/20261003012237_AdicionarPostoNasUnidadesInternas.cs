using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPostoNasUnidadesInternas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Funcionarios_FuncionarioId",
                table: "Usuarios");

            migrationBuilder.AddColumn<int>(
                name: "PostoId",
                table: "UnidadesOperacionais",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesOperacionais_PostoId",
                table: "UnidadesOperacionais",
                column: "PostoId");

            migrationBuilder.AddForeignKey(
                name: "FK_UnidadesOperacionais_Postos_PostoId",
                table: "UnidadesOperacionais",
                column: "PostoId",
                principalTable: "Postos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Funcionarios_FuncionarioId",
                table: "Usuarios",
                column: "FuncionarioId",
                principalTable: "Funcionarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnidadesOperacionais_Postos_PostoId",
                table: "UnidadesOperacionais");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Funcionarios_FuncionarioId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_UnidadesOperacionais_PostoId",
                table: "UnidadesOperacionais");

            migrationBuilder.DropColumn(
                name: "PostoId",
                table: "UnidadesOperacionais");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Funcionarios_FuncionarioId",
                table: "Usuarios",
                column: "FuncionarioId",
                principalTable: "Funcionarios",
                principalColumn: "Id");
        }
    }
}
