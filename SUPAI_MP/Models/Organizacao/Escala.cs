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

        // Opcional:
        // usado principalmente quando o funcionário
        // está escalado para um posto específico.
        public int? PostoId { get; set; }

        [StringLength(300)]
        public string? Observacao { get; set; }

        public Funcionario? Funcionario { get; set; }

        public TipoTurno? TipoTurno { get; set; }

        public Posto? Posto { get; set; }
    }
}