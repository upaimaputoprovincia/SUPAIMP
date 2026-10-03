using System.ComponentModel.DataAnnotations;
using supai_mp.Models;

namespace supai_mp.Models.Institucional
{
    public class PermissaoAcesso
    {
        public int Id { get; set; }

        // ============================================================
        // UTILIZADOR
        // ============================================================

        [Required]
        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        // ============================================================
        // PERMISSÃO
        // ============================================================

        [Required]
        public int TipoPermissaoId { get; set; }

        public TipoPermissao TipoPermissao { get; set; } = null!;

        // ============================================================
        // ÂMBITO INSTITUCIONAL
        // ============================================================

        [Required]
        public int UnidadeInstitucionalId { get; set; }

        public UnidadeInstitucional UnidadeInstitucional { get; set; }
            = null!;

        // ============================================================
        // ESTADO
        // ============================================================

        public bool Ativo { get; set; } = true;

        public DateTime DataConcessao { get; set; } = DateTime.Now;

        public DateTime? DataExpiracao { get; set; }

        [MaxLength(300)]
        public string? Observacao { get; set; }
    }
}