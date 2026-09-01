using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.DTOs
{
    public class CriarFeriasDto
    {
        [Required]
        [Range(2020, 2100)]
        public int Ano { get; set; }

        [Required]
        [Range(1, 12)]
        public int Mes { get; set; }

        [StringLength(500)]
        public string? Observacao { get; set; }
    }
}