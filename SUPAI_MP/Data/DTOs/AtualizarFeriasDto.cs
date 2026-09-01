using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.DTOs
{
    public class AtualizarFeriasDto
    {
        [Required]
        public int Ano { get; set; }

        [Required]
        [Range(1, 12)]
        public int Mes { get; set; }

        public DateTime? DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        [StringLength(500)]
        public string? Observacao { get; set; }
    }
}