public class GerarEscalaResultadoDto
{
    public string Mensagem { get; set; } = string.Empty;

    public int UnidadeId { get; set; }

    public string? UnidadeNome { get; set; }

    // ============================================================
    // ESCOLTA
    // ============================================================

    public int? GrupoId { get; set; }

    public string? GrupoNome { get; set; }

    public int? OrdemRotacao { get; set; }

    public int? EquipaId { get; set; }

    public string? EquipaNome { get; set; }

    // ============================================================
    // PROTECÇÃO DE OBJECTOS
    // ============================================================

    public int? CompanhiaId { get; set; }

    public string? CompanhiaNome { get; set; }

    public string? TipoUnidadeServico { get; set; }

    public int? UnidadeServicoId { get; set; }

    public string? UnidadeServicoNome { get; set; }

    public int QuantidadePostos { get; set; }

    public List<PostoEscalaResultadoDto> PostosDistribuidos { get; set; }
        = new();

    // ============================================================
    // ESCALA
    // ============================================================

    public DateTime Data { get; set; }

    public TimeSpan HoraInicio { get; set; }

    public TimeSpan HoraFim { get; set; }

    public int TipoTurnoId { get; set; }

    public string? TipoTurnoNome { get; set; }

    public int FuncionariosNaEquipa { get; set; }

    public int FuncionariosComLotacaoValida { get; set; }

    public int QuantidadeFuncionariosComTurnoInvalido { get; set; }

    public int EscalasCriadas { get; set; }

    public int EscalasJaExistentes { get; set; }

    public int QuantidadeFuncionariosComConflito { get; set; }

    public List<FuncionarioEscalaResultadoDto> FuncionariosEscalados { get; set; }
        = new();

    public List<FuncionarioEscalaResultadoDto> FuncionariosJaEscalados { get; set; }
        = new();

    public List<FuncionarioEscalaResultadoDto> FuncionariosComConflito { get; set; }
        = new();

    public List<FuncionarioEscalaResultadoDto> FuncionariosComTurnoInvalido { get; set; }
        = new();
}

public class PostoEscalaResultadoDto
{
    public int PostoId { get; set; }

    public string? Codigo { get; set; }

    public string? Nome { get; set; }

    public int QuantidadeFuncionarios { get; set; }

    public List<FuncionarioEscalaResultadoDto> Funcionarios { get; set; }
        = new();
}

