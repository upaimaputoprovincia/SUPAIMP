
using System.ComponentModel.DataAnnotations;
using supai_mp.Models.Organizacao;

namespace supai_mp.Models.Institucional
{
    public class UnidadeInstitucionalSeccao
    {
        public int Id { get; set; }

        [Required]
        public int UnidadeInstitucionalId { get; set; }

        public UnidadeInstitucional UnidadeInstitucional { get; set; }
            = null!;

        [Required]
        public int SeccaoId { get; set; }

        public Seccao Seccao { get; set; }
            = null!;

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [StringLength(300)]
        public string? Observacao { get; set; }
    }
}

