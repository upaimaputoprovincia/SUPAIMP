using System.ComponentModel.DataAnnotations;
using supai_mp.Models;

namespace supai_mp.Models.Organizacao
{
    public class LotacaoFuncionario
    {
        public int Id { get; set; }

        [Required]
        public int FuncionarioId { get; set; }

        
        public int? SeccaoId { get; set; }

        public int? SectorId { get; set; }

        public int? UnidadeOperacionalId { get; set; }

        public int? EquipaId { get; set; }

        public int? PostoId { get; set; }

        // Todo funcionário lotado deve ter uma função.
        [Required]
        public int FuncaoOperacionalId { get; set; }

        public int? TipoTurnoId { get; set; }

        public DateTime DataInicio { get; set; } = DateTime.Now;

        public DateTime? DataFim { get; set; }

        public bool Ativo { get; set; } = true;

        [StringLength(200)]
        public string? Motivo { get; set; }

        [StringLength(500)]
        public string? Observacao { get; set; }

        public Funcionario? Funcionario { get; set; }

        public Seccao? Seccao { get; set; }

        public Sector? Sector { get; set; }

        public UnidadeOperacional? UnidadeOperacional { get; set; }

        public Equipa? Equipa { get; set; }

        public Posto? Posto { get; set; }

        public FuncaoOperacional? FuncaoOperacional { get; set; }

        public TipoTurno? TipoTurno { get; set; }
    }
}