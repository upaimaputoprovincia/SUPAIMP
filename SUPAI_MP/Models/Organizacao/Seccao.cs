using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace supai_mp.Models.Organizacao
{
    public class Seccao
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [JsonIgnore]
        public ICollection<Sector> Sectores { get; set; } = new List<Sector>();

        [JsonIgnore]
        public ICollection<Posto> Postos { get; set; } = new List<Posto>();

        [JsonIgnore]
        public ICollection<LotacaoFuncionario> Lotacoes { get; set; }
            = new List<LotacaoFuncionario>();
    }
}