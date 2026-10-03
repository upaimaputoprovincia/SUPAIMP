using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarInfraestruturaInstitucional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntidadesInstitucionais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sigla = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntidadesInstitucionais", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TiposUnidadeInstitucional",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposUnidadeInstitucional", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UnidadesInstitucionais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sigla = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoUnidadeInstitucionalId = table.Column<int>(type: "int", nullable: false),
                    EntidadeInstitucionalId = table.Column<int>(type: "int", nullable: false),
                    UnidadePaiId = table.Column<int>(type: "int", nullable: true),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesInstitucionais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnidadesInstitucionais_EntidadesInstitucionais_EntidadeInsti~",
                        column: x => x.EntidadeInstitucionalId,
                        principalTable: "EntidadesInstitucionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnidadesInstitucionais_TiposUnidadeInstitucional_TipoUnidade~",
                        column: x => x.TipoUnidadeInstitucionalId,
                        principalTable: "TiposUnidadeInstitucional",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnidadesInstitucionais_UnidadesInstitucionais_UnidadePaiId",
                        column: x => x.UnidadePaiId,
                        principalTable: "UnidadesInstitucionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EntidadesInstitucionais_Nome",
                table: "EntidadesInstitucionais",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposUnidadeInstitucional_Nome",
                table: "TiposUnidadeInstitucional",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesInstitucionais_EntidadeInstitucionalId_Nome",
                table: "UnidadesInstitucionais",
                columns: new[] { "EntidadeInstitucionalId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesInstitucionais_TipoUnidadeInstitucionalId",
                table: "UnidadesInstitucionais",
                column: "TipoUnidadeInstitucionalId");

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesInstitucionais_UnidadePaiId",
                table: "UnidadesInstitucionais",
                column: "UnidadePaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnidadesInstitucionais");

            migrationBuilder.DropTable(
                name: "EntidadesInstitucionais");

            migrationBuilder.DropTable(
                name: "TiposUnidadeInstitucional");
        }
    }
}
