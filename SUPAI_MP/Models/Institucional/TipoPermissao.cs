using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Institucional
{
    public class TipoPermissao
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<PermissaoAcesso> Permissoes { get; set; }
            = new List<PermissaoAcesso>();
    }
}