using supai_mp.Models.Organizacao;
using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Organizacao
{
    public class FuncaoOperacional
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<LotacaoFuncionario> Lotacoes { get; set; }
            = new List<LotacaoFuncionario>();
    }
}