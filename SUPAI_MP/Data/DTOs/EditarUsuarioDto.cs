using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.DTOs
{
    public class EditarUsuarioDto
    {
        // ============================================================
        // CREDENCIAIS
        // ============================================================

        [Required(ErrorMessage = "O nome de utilizador é obrigatório.")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "O nome de utilizador deve ter entre 3 e 100 caracteres.")]
        public string NomeUsuario { get; set; } = string.Empty;

        // ============================================================
        // PERFIL
        // ============================================================

        [Required(ErrorMessage = "O perfil é obrigatório.")]
        public string Perfil { get; set; } = string.Empty;

        // ============================================================
        // FUNCIONÁRIO
        // ============================================================
        //
        // Obrigatório para utilizadores internos.
        // Nulo para utilizadores institucionais externos.
        //
        // ============================================================

        public int? FuncionarioId { get; set; }

        // ============================================================
        // UNIDADE INSTITUCIONAL
        // ============================================================
        //
        // Unidade à qual o utilizador institucional pertence.
        //
        // ============================================================

        public int? UnidadeInstitucionalId { get; set; }
    }
}