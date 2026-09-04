using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Organizacao
{
    public class Equipa
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        // Uma equipa pode pertencer a um Sector
        // ou directamente a uma Unidade Operacional.
        public int? SectorId { get; set; }

        public int? UnidadeOperacionalId { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public Sector? Sector { get; set; }

        public UnidadeOperacional? UnidadeOperacional { get; set; }

        public ICollection<LotacaoFuncionario> Lotacoes { get; set; }
            = new List<LotacaoFuncionario>();
    }
}