using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Institucional
{
    public class TipoUnidadeInstitucional
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<UnidadeInstitucional> Unidades { get; set; }
            = new List<UnidadeInstitucional>();
    }
}