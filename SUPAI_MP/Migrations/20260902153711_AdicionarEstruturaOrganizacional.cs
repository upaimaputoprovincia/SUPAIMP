using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPAI_MP.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEstruturaOrganizacional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FuncoesOperacionais",
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
                    table.PrimaryKey("PK_FuncoesOperacionais", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Seccoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seccoes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TiposTurno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorasTrabalho = table.Column<int>(type: "int", nullable: false),
                    HorasDescanso = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposTurno", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sectores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SeccaoId = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sectores_Seccoes_SeccaoId",
                        column: x => x.SeccaoId,
                        principalTable: "Seccoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Equipas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SectorId = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipas_Sectores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Postos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Localizacao = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SeccaoId = table.Column<int>(type: "int", nullable: false),
                    SectorId = table.Column<int>(type: "int", nullable: true),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Postos_Seccoes_SeccaoId",
                        column: x => x.SeccaoId,
                        principalTable: "Seccoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Postos_Sectores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Escalas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FuncionarioId = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    TipoTurnoId = table.Column<int>(type: "int", nullable: false),
                    PostoId = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escalas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Escalas_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Escalas_Postos_PostoId",
                        column: x => x.PostoId,
                        principalTable: "Postos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Escalas_TiposTurno_TipoTurnoId",
                        column: x => x.TipoTurnoId,
                        principalTable: "TiposTurno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LotacoesFuncionarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FuncionarioId = table.Column<int>(type: "int", nullable: false),
                    SeccaoId = table.Column<int>(type: "int", nullable: false),
                    SectorId = table.Column<int>(type: "int", nullable: true),
                    EquipaId = table.Column<int>(type: "int", nullable: true),
                    PostoId = table.Column<int>(type: "int", nullable: true),
                    FuncaoOperacionalId = table.Column<int>(type: "int", nullable: true),
                    TipoTurnoId = table.Column<int>(type: "int", nullable: true),
                    DataInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Motivo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observacao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotacoesFuncionarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotacoesFuncionarios_Equipas_EquipaId",
                        column: x => x.EquipaId,
                        principalTable: "Equipas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotacoesFuncionarios_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotacoesFuncionarios_FuncoesOperacionais_FuncaoOperacionalId",
                        column: x => x.FuncaoOperacionalId,
                        principalTable: "FuncoesOperacionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotacoesFuncionarios_Postos_PostoId",
                        column: x => x.PostoId,
                        principalTable: "Postos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotacoesFuncionarios_Seccoes_SeccaoId",
                        column: x => x.SeccaoId,
                        principalTable: "Seccoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotacoesFuncionarios_Sectores_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotacoesFuncionarios_TiposTurno_TipoTurnoId",
                        column: x => x.TipoTurnoId,
                        principalTable: "TiposTurno",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Equipas_SectorId",
                table: "Equipas",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_FuncionarioId_Data_HoraInicio",
                table: "Escalas",
                columns: new[] { "FuncionarioId", "Data", "HoraInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_PostoId",
                table: "Escalas",
                column: "PostoId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalas_TipoTurnoId",
                table: "Escalas",
                column: "TipoTurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_FuncoesOperacionais_Nome",
                table: "FuncoesOperacionais",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LotacoesFuncionarios_EquipaId",
                table: "LotacoesFuncionarios",
                column: "EquipaId");

            migrationBuilder.CreateIndex(
                name: "IX_LotacoesFuncionarios_FuncaoOperacionalId",
                table: "LotacoesFuncionarios",
                column: "FuncaoOperacionalId");

            migrationBuilder.CreateIndex(
                name: "IX_LotacoesFuncionarios_FuncionarioId",
                table: "LotacoesFuncionarios",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_LotacoesFuncionarios_PostoId",
                table: "LotacoesFuncionarios",
                column: "PostoId");

            migrationBuilder.CreateIndex(
                name: "IX_LotacoesFuncionarios_SeccaoId",
                table: "LotacoesFuncionarios",
                column: "SeccaoId");

            migrationBuilder.CreateIndex(
                name: "IX_LotacoesFuncionarios_SectorId",
                table: "LotacoesFuncionarios",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_LotacoesFuncionarios_TipoTurnoId",
                table: "LotacoesFuncionarios",
                column: "TipoTurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Postos_Codigo",
                table: "Postos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Postos_SeccaoId",
                table: "Postos",
                column: "SeccaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Postos_SectorId",
                table: "Postos",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Seccoes_Nome",
                table: "Seccoes",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sectores_SeccaoId",
                table: "Sectores",
                column: "SeccaoId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposTurno_Nome",
                table: "TiposTurno",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Escalas");

            migrationBuilder.DropTable(
                name: "LotacoesFuncionarios");

            migrationBuilder.DropTable(
                name: "Equipas");

            migrationBuilder.DropTable(
                name: "FuncoesOperacionais");

            migrationBuilder.DropTable(
                name: "Postos");

            migrationBuilder.DropTable(
                name: "TiposTurno");

            migrationBuilder.DropTable(
                name: "Sectores");

            migrationBuilder.DropTable(
                name: "Seccoes");
        }
    }
}
