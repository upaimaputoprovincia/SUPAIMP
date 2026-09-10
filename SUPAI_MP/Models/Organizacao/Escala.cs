using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Organizacao
{
    public class Escala
    {
        public int Id { get; set; }

        [Required]
        public int FuncionarioId { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFim { get; set; }

        [Required]
        public int TipoTurnoId { get; set; }

        // Snapshot da lotação no momento da criação da escala
        public int? SeccaoId { get; set; }

        public int? UnidadeOperacionalId { get; set; }

        public int? EquipaId { get; set; }

        [Required]
        public int FuncaoOperacionalId { get; set; }

        // Posto efetivamente usado na escala
        public int? PostoId { get; set; }

        [StringLength(300)]
        public string? Observacao { get; set; }

        // Navegações
        public Funcionario? Funcionario { get; set; }

        public TipoTurno? TipoTurno { get; set; }

        public Seccao? Seccao { get; set; }

        public UnidadeOperacional? UnidadeOperacional { get; set; }

        public Equipa? Equipa { get; set; }

        public FuncaoOperacional? FuncaoOperacional { get; set; }

        public Posto? Posto { get; set; }
    }
}