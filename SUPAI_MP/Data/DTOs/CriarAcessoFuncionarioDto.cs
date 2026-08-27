using System.ComponentModel.DataAnnotations;

namespace SUPAI_MP.Data.DTOs
{
    public class CriarAcessoFuncionarioDto
    {
        [Required]
        public string NomeUsuario { get; set; } = string.Empty;

        [Required]
        public string Senha { get; set; } = string.Empty;
    }
}
