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
            // ============================================================
            // 1. CORRIGIR AS ESCALAS EXISTENTES
            // ============================================================
            //
            // A primeira tentativa da migration já criou a coluna
            // FuncaoOperacionalId, mas as escalas existentes ficaram
            // temporariamente com valor 0.
            //
            // Vamos recuperar a função correta a partir da lotação
            // válida do funcionário na data da escala.
            //
            // Regra:
            // - mesmo funcionário
            // - DataInicio da lotação <= data da escala
            // - DataFim nula OU posterior à data da escala
            // - lotação ativa
            // - função existente
            // - se houver mais de uma, usar a mais recente
            // ============================================================

            migrationBuilder.Sql(@"
                UPDATE `Escalas` e
                SET e.`FuncaoOperacionalId` = (
                    SELECT l.`FuncaoOperacionalId`
                    FROM `LotacoesFuncionarios` l
                    INNER JOIN `FuncoesOperacionais` f
                        ON f.`Id` = l.`FuncaoOperacionalId`
                    WHERE l.`FuncionarioId` = e.`FuncionarioId`
                      AND l.`Ativo` = 1
                      AND DATE(l.`DataInicio`) <= DATE(e.`Data`)
                      AND (
                            l.`DataFim` IS NULL
                            OR DATE(l.`DataFim`) > DATE(e.`Data`)
                          )
                      AND l.`FuncaoOperacionalId` > 0
                    ORDER BY l.`DataInicio` DESC
                    LIMIT 1
                )
                WHERE e.`FuncaoOperacionalId` = 0;
            ");

            // ============================================================
            // 2. REMOVER O DEFAULT 0 DA COLUNA
            // ============================================================
            //
            // A coluna foi criada originalmente com:
            //     nullable: false
            //     defaultValue: 0
            //
            // Agora que os dados foram corrigidos, removemos esse
            // comportamento para impedir que novas escalas sejam
            // gravadas com função 0.
            // ============================================================

            migrationBuilder.Sql(@"
                ALTER TABLE `Escalas`
                MODIFY COLUMN `FuncaoOperacionalId` int NOT NULL;
            ");

            // ============================================================
            // 3. CRIAR A FK DE FUNÇÃO OPERACIONAL
            // ============================================================

            migrationBuilder.AddForeignKey(
                name: "FK_Escalas_FuncoesOperacionais_FuncaoOperacionalId",
                table: "Escalas",
                column: "FuncaoOperacionalId",
                principalTable: "FuncoesOperacionais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ============================================================
            // 4. CRIAR A FK DA SECÇÃO
            // ============================================================
            //
            // A coluna e o índice já existem fisicamente.
            // Apenas falta a FK.
            // ============================================================

            migrationBuilder.AddForeignKey(
                name: "FK_Escalas_Seccoes_SeccaoId",
                table: "Escalas",
                column: "SeccaoId",
                principalTable: "Seccoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ============================================================
            // 5. CRIAR A FK DA UNIDADE OPERACIONAL
            // ============================================================

            migrationBuilder.AddForeignKey(
                name: "FK_Escalas_UnidadesOperacionais_UnidadeOperacionalId",
                table: "Escalas",
                column: "UnidadeOperacionalId",
                principalTable: "UnidadesOperacionais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ============================================================
            // IMPORTANTE:
            //
            // NÃO adicionamos:
            // - EquipaId
            // - FuncaoOperacionalId
            // - SeccaoId
            // - UnidadeOperacionalId
            //
            // porque essas colunas já existem no banco.
            //
            // NÃO criamos índices porque eles também já existem.
            //
            // NÃO criamos a FK de Equipa porque ela já foi criada
            // durante a primeira execução parcial da migration.
            // ============================================================
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover apenas as FKs desta etapa de reparação.
            //
            // As colunas, índices e dados NÃO são removidos, pois
            // já existiam fisicamente antes desta migration ser
            // registrada no histórico do EF.

            migrationBuilder.DropForeignKey(
                name: "FK_Escalas_FuncoesOperacionais_FuncaoOperacionalId",
                table: "Escalas");

            migrationBuilder.DropForeignKey(
                name: "FK_Escalas_Seccoes_SeccaoId",
                table: "Escalas");

            migrationBuilder.DropForeignKey(
                name: "FK_Escalas_UnidadesOperacionais_UnidadeOperacionalId",
                table: "Escalas");
        }
    }
}