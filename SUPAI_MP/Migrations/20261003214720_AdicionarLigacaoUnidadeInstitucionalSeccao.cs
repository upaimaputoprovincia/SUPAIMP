using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLigacaoUnidadeInstitucionalSeccao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnidadesInstitucionaisSeccoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UnidadeInstitucionalId = table.Column<int>(type: "int", nullable: false),
                    SeccaoId = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Observacao = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesInstitucionaisSeccoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnidadesInstitucionaisSeccoes_Seccoes_SeccaoId",
                        column: x => x.SeccaoId,
                        principalTable: "Seccoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnidadesInstitucionaisSeccoes_UnidadesInstitucionais_Unidade~",
                        column: x => x.UnidadeInstitucionalId,
                        principalTable: "UnidadesInstitucionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesInstitucionaisSeccoes_SeccaoId",
                table: "UnidadesInstitucionaisSeccoes",
                column: "SeccaoId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesInstitucionaisSeccoes_UnidadeInstitucionalId_SeccaoId",
                table: "UnidadesInstitucionaisSeccoes",
                columns: new[] { "UnidadeInstitucionalId", "SeccaoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnidadesInstitucionaisSeccoes");
        }
    }
}
