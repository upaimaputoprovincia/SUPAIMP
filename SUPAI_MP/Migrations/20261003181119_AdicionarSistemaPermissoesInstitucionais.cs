using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSistemaPermissoesInstitucionais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposPermissao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPermissao", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PermissoesAcesso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TipoPermissaoId = table.Column<int>(type: "int", nullable: false),
                    UnidadeInstitucionalId = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataConcessao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Observacao = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissoesAcesso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissoesAcesso_TiposPermissao_TipoPermissaoId",
                        column: x => x.TipoPermissaoId,
                        principalTable: "TiposPermissao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermissoesAcesso_UnidadesInstitucionais_UnidadeInstitucional~",
                        column: x => x.UnidadeInstitucionalId,
                        principalTable: "UnidadesInstitucionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermissoesAcesso_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "TiposPermissao",
                columns: new[] { "Id", "Ativo", "Codigo", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, true, "CONSULTAR_EFECTIVO", "Permite consultar a informação do efectivo da unidade autorizada.", "Consultar Efectivo" },
                    { 2, true, "CONSULTAR_POSTOS", "Permite consultar a situação dos postos da unidade autorizada.", "Consultar Postos" },
                    { 3, true, "CONSULTAR_SEGURANCA", "Permite consultar informação de situação de segurança autorizada.", "Consultar Segurança" },
                    { 4, true, "CONSULTAR_RELATORIOS", "Permite consultar relatórios disponibilizados à unidade autorizada.", "Consultar Relatórios" },
                    { 5, true, "CONSULTAR_ESCALAS", "Permite consultar escalas da unidade autorizada.", "Consultar Escalas" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissoesAcesso_TipoPermissaoId",
                table: "PermissoesAcesso",
                column: "TipoPermissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissoesAcesso_UnidadeInstitucionalId",
                table: "PermissoesAcesso",
                column: "UnidadeInstitucionalId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissoesAcesso_UsuarioId_TipoPermissaoId_UnidadeInstitucio~",
                table: "PermissoesAcesso",
                columns: new[] { "UsuarioId", "TipoPermissaoId", "UnidadeInstitucionalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposPermissao_Codigo",
                table: "TiposPermissao",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissoesAcesso");

            migrationBuilder.DropTable(
                name: "TiposPermissao");
        }
    }
}
