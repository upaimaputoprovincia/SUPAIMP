using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Organizacao
{
    public class UnidadeOperacional
    {
        public enum TipoUnidadeOperacional
        {
            Escolta = 0,
            Companhia = 1,
            Pelotao = 2
        }

        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public TipoUnidadeOperacional Tipo { get; set; }

        [Required]
        public int SeccaoId { get; set; }

        public int? UnidadePaiId { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public Seccao? Seccao { get; set; }

        public UnidadeOperacional? UnidadePai { get; set; }

        public ICollection<UnidadeOperacional> UnidadesFilhas { get; set; }
            = new List<UnidadeOperacional>();

        public ICollection<LotacaoFuncionario> Lotacoes { get; set; }
            = new List<LotacaoFuncionario>();

        public ICollection<Posto> Postos { get; set; }
            = new List<Posto>();

        public ICollection<Equipa> Equipas { get; set; }
            = new List<Equipa>();
    }
}