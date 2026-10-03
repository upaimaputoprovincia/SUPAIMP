using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Organizacao
{
    public class Posto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Localizacao { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        public int SeccaoId { get; set; }

        public int? SectorId { get; set; }

        public int? UnidadeOperacionalId { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // =========================================================
        // NAVEGAÇÕES
        // =========================================================

        public Seccao? Seccao { get; set; }

        public Sector? Sector { get; set; }

        public UnidadeOperacional? UnidadeOperacional { get; set; }

        public ICollection<LotacaoFuncionario> Lotacoes { get; set; }
            = new List<LotacaoFuncionario>();

        // Secções Internas associadas a este Posto
        public ICollection<UnidadeOperacional> UnidadesInternas { get; set; }
            = new List<UnidadeOperacional>();
    }
}

