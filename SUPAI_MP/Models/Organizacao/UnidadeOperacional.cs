using System.ComponentModel.DataAnnotations;
using supai_mp.Models.Organizacao;

namespace supai_mp.Models.Organizacao
{
    public class UnidadeOperacional
    {
        public enum TipoUnidadeOperacional
        {
            Escolta = 0,
            Companhia = 1,
            Pelotao = 2,
            SeccaoInterna = 3
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

        // =========================================================
        // POSTO ASSOCIADO
        // Usado principalmente pelas Secções Internas da
        // Protecção de Objectos.
        // =========================================================
        public int? PostoId { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // =========================================================
        // NAVEGAÇÕES
        // =========================================================

        public Seccao? Seccao { get; set; }

        public UnidadeOperacional? UnidadePai { get; set; }

        public Posto? Posto { get; set; }

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
