using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Institucional
{
    public class EntidadeInstitucional
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Sigla { get; set; }

        [MaxLength(300)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<UnidadeInstitucional> Unidades { get; set; }
            = new List<UnidadeInstitucional>();
    }
}