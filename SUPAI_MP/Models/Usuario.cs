using System.ComponentModel.DataAnnotations;
using supai_mp.Models.Institucional;

namespace supai_mp.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        // ============================================================
        // CREDENCIAIS
        // ============================================================

        [Required]
        public string NomeUsuario { get; set; } = string.Empty;

        [Required]
        public string SenhaHash { get; set; } = string.Empty;

        // ============================================================
        // PERFIL
        // ============================================================

        [Required]
        public string Perfil { get; set; } = string.Empty;

        // ============================================================
        // FUNCIONÁRIO
        // ============================================================

        public int? FuncionarioId { get; set; }

        public Funcionario? Funcionario { get; set; }

        // ============================================================
        // UNIDADE INSTITUCIONAL
        // ============================================================

        public int? UnidadeInstitucionalId { get; set; }

        public UnidadeInstitucional? UnidadeInstitucional { get; set; }

        // ============================================================
        // ESTADO
        // ============================================================

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}