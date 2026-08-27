using supai_mp.Models;
using System.ComponentModel.DataAnnotations;

namespace SUPAI_MP.Data.DTOs
{
    public class FuncionarioUpdateDto
    {
        [Required]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        public string Nip { get; set; } = string.Empty;

        [Required]
        public string Bi { get; set; } = string.Empty;

        [Required]
        public string Nuit { get; set; } = string.Empty;

        [Required]
        public Genero Genero { get; set; }

        [Required]
        public EstadoCivil EstadoCivil { get; set; }

        [Required]
        public NivelAcademico NivelAcademico { get; set; }

        [Required]
        public GrauParentesco GrauParentesco { get; set; }

        [Required]
        public string Contacto { get; set; } = string.Empty;

        public string? C_Alternativo { get; set; }

        public string? C_Familiar { get; set; }

        [Required]
        public DateTime DataNascimento { get; set; }

        [Required]
        public DateTime DataIngresso { get; set; }

        [Required]
        public string LocalTrabalho { get; set; } = string.Empty;

        public string? Bairro { get; set; }

        public string? Quarterao_N { get; set; }

        public string? Casa_N { get; set; }

        [Required]
        public Categoria Categoria { get; set; }

        [Required]
        public string Funcao { get; set; } = string.Empty;

        [Required]
        public EstadoFuncionario EstadoFuncionario { get; set; }
    }
}
