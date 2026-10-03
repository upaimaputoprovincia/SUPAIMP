using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.DTOs
{
    public class CriarUsuarioDto
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

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(
            6,
            ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
        public string Senha { get; set; } = string.Empty;

        // ============================================================
        // PERFIL
        // ============================================================

        [Required(ErrorMessage = "O perfil é obrigatório.")]
        public string Perfil { get; set; } = "Funcionario";

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
        // Utilizado para associar o utilizador à sua unidade
        // institucional.
        //
        // Exemplo:
        // Comando Provincial da PRM – Maputo
        //
        // ============================================================

        public int? UnidadeInstitucionalId { get; set; }
    }
}