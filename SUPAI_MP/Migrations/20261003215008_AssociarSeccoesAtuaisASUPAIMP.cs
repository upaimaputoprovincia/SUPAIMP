using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AssociarSeccoesAtuaisASUPAIMP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO UnidadesInstitucionaisSeccoes
                    (UnidadeInstitucionalId, SeccaoId, Ativo, DataCadastro)
                SELECT
                    2,
                    s.Id,
                    1,
                    NOW()
                FROM Seccoes s
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM UnidadesInstitucionaisSeccoes uis
                    WHERE uis.UnidadeInstitucionalId = 2
                      AND uis.SeccaoId = s.Id
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql(@"
                DELETE FROM UnidadesInstitucionaisSeccoes
                WHERE UnidadeInstitucionalId = 2;
            ");
        }
    }
}
