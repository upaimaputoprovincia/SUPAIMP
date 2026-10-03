using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class SeedInfraestruturaInstitucionalInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EntidadesInstitucionais",
                columns: new[] { "Id", "Ativo", "Descricao", "Nome", "Sigla" },
                values: new object[] { 1, true, "Comando Provincial da Polícia da República de Moçambique – Maputo", "Comando Provincial da PRM – Maputo", "CPRM-Maputo" });

            migrationBuilder.InsertData(
                table: "TiposUnidadeInstitucional",
                columns: new[] { "Id", "Ativo", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, true, "Unidade institucional correspondente ao Comando Provincial.", "Comando Provincial" },
                    { 2, true, "Subunidade institucional subordinada ao Comando Provincial.", "Subunidade" },
                    { 3, true, "Esquadra policial integrada na estrutura institucional.", "Esquadra" },
                    { 4, true, "Posto policial integrado na estrutura institucional.", "Posto" }
                });

            migrationBuilder.InsertData(
                table: "UnidadesInstitucionais",
                columns: new[] { "Id", "Ativo", "DataCadastro", "Descricao", "EntidadeInstitucionalId", "Nome", "Sigla", "TipoUnidadeInstitucionalId", "UnidadePaiId" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Comando Provincial da Polícia da República de Moçambique – Maputo.", 1, "Comando Provincial da PRM – Maputo", "CPRM-Maputo", 1, null },
                    { 2, true, new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Subunidade de Protecção de Altas Individualidades.", 1, "SUPAI-MP", "SUPAI-MP", 2, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TiposUnidadeInstitucional",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TiposUnidadeInstitucional",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UnidadesInstitucionais",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TiposUnidadeInstitucional",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UnidadesInstitucionais",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EntidadesInstitucionais",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TiposUnidadeInstitucional",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
