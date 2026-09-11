using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSeccaoAoFuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeccaoId",
                table: "Funcionarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_SeccaoId",
                table: "Funcionarios",
                column: "SeccaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Funcionarios_Seccoes_SeccaoId",
                table: "Funcionarios",
                column: "SeccaoId",
                principalTable: "Seccoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Funcionarios_Seccoes_SeccaoId",
                table: "Funcionarios");

            migrationBuilder.DropIndex(
                name: "IX_Funcionarios_SeccaoId",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "SeccaoId",
                table: "Funcionarios");
        }
    }
}
