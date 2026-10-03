using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AssociarUsuarioAUnidadeInstitucional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnidadeInstitucionalId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UnidadeInstitucionalId",
                table: "Usuarios",
                column: "UnidadeInstitucionalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_UnidadesInstitucionais_UnidadeInstitucionalId",
                table: "Usuarios",
                column: "UnidadeInstitucionalId",
                principalTable: "UnidadesInstitucionais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_UnidadesInstitucionais_UnidadeInstitucionalId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_UnidadeInstitucionalId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UnidadeInstitucionalId",
                table: "Usuarios");
        }
    }
}
