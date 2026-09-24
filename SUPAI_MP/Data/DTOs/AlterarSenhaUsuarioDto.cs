using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.DTOs
{
    public class AlterarSenhaUsuarioDto
    {
        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [MinLength(
            6,
            ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.")]
        public string NovaSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a nova senha.")]
        [Compare(
            "NovaSenha",
            ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmarNovaSenha { get; set; } = string.Empty;
    }
}