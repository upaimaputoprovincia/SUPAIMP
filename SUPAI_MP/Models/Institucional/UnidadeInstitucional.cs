using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Institucional
{
    public class UnidadeInstitucional
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Sigla { get; set; }

        [MaxLength(300)]
        public string? Descricao { get; set; }

        // ============================================================
        // TIPO DA UNIDADE
        // ============================================================

        [Required]
        public int TipoUnidadeInstitucionalId { get; set; }

        public TipoUnidadeInstitucional TipoUnidadeInstitucional { get; set; }
            = null!;

        // ============================================================
        // ENTIDADE INSTITUCIONAL
        // ============================================================

        [Required]
        public int EntidadeInstitucionalId { get; set; }

        public EntidadeInstitucional EntidadeInstitucional { get; set; }
            = null!;

        // ============================================================
        // HIERARQUIA INSTITUCIONAL
        // ============================================================

        public int? UnidadePaiId { get; set; }

        public UnidadeInstitucional? UnidadePai { get; set; }

        public ICollection<UnidadeInstitucional> UnidadesFilhas { get; set; }
            = new List<UnidadeInstitucional>();

        // ============================================================
        // ESTADO
        // ============================================================

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}