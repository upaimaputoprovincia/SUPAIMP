using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Organizacao
{
    public class TipoTurno
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Descricao { get; set; }

        public int HorasTrabalho { get; set; }

        public int HorasDescanso { get; set; }

        public bool Ativo { get; set; } = true;
    }
}