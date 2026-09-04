using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace supai_mp.Models.Organizacao
{
    public class Sector
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public int SeccaoId { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [JsonIgnore]
        public Seccao? Seccao { get; set; }

        public ICollection<Equipa> Equipas { get; set; } = new List<Equipa>();

        public ICollection<Posto> Postos { get; set; } = new List<Posto>();

        public ICollection<LotacaoFuncionario> Lotacoes { get; set; }
            = new List<LotacaoFuncionario>();
    }
}