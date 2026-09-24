using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.DTOs
{
    public class EditarUsuarioDto
    {
        [Required(ErrorMessage = "O nome de utilizador é obrigatório.")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "O nome de utilizador deve ter entre 3 e 100 caracteres.")]
        public string NomeUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "O perfil é obrigatório.")]
        public string Perfil { get; set; } = string.Empty;

        [Required(ErrorMessage = "O funcionário é obrigatório.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Selecione um funcionário válido.")]
        public int FuncionarioId { get; set; }
    }
}