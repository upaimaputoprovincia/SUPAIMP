using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        public string NomeUsuario { get; set; } = string.Empty;

        [Required]
        public string SenhaHash { get; set; } = string.Empty;

        [Required]
        public string Perfil { get; set; } = string.Empty;

        public int? FuncionarioId { get; set; }

        public Funcionario? Funcionario { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}