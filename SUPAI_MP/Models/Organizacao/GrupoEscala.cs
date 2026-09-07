using System.ComponentModel.DataAnnotations;

namespace supai_mp.Models.Organizacao
{
    public class GrupoEscala
    {
        public int Id { get; set; }

        // Nome do grupo: Alfa, Beta ou Charlie
        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        // Ordem do grupo dentro da rotação:
        // 1 = Alfa
        // 2 = Beta
        // 3 = Charlie
        [Required]
        public int OrdemRotacao { get; set; }

        // Unidade operacional à qual o grupo pertence.
        // Ex.: Escolta A ou Escolta C
        [Required]
        public int UnidadeOperacionalId { get; set; }

        // Equipa associada ao grupo.
        // Ex.: Equipa Alfa, Equipa Beta ou Equipa Charlie
        [Required]
        public int EquipaId { get; set; }

        // Tipo de turno utilizado pelo grupo.
        // Neste caso será o turno 24/48.
        [Required]
        public int TipoTurnoId { get; set; }

        // Data que serve como referência para calcular a rotação.
        [Required]
        public DateTime DataReferencia { get; set; }

        // Indica se este grupo está atualmente em utilização.
        public bool Ativo { get; set; } = true;

        [StringLength(500)]
        public string? Observacao { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Navegações
        public UnidadeOperacional? UnidadeOperacional { get; set; }

        public Equipa? Equipa { get; set; }

        public TipoTurno? TipoTurno { get; set; }
    }
}