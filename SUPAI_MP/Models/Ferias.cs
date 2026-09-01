using System.ComponentModel.DataAnnotations;



namespace supai_mp.Models
{
    public enum EstadoFerias
    {
        Programada = 0,
        EmGozo = 1,
        Gozada = 2,
        Cancelada = 3
    }

    public class Ferias
    {
        public int Id { get; set; }

        [Required]
        public int FuncionarioId { get; set; }

        [Required]
        public int Ano { get; set; }

        [Required]
        public int Mes { get; set; }

        public DateTime? DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public EstadoFerias Estado { get; set; } = EstadoFerias.Programada;

        public DateTime DataMarcacao { get; set; } = DateTime.Now;

        public DateTime? DataRegistoInicio { get; set; }

        public DateTime? DataRegistoFim { get; set; }

        [StringLength(500)]
        public string? Observacao { get; set; }

        // Relação com o funcionário
        public Funcionario? Funcionario { get; set; }
    }
}