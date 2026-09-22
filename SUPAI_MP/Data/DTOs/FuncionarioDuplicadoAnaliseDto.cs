public class FuncionarioDuplicadoAnaliseDto
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Nip { get; set; } = string.Empty;

    public int? SeccaoId { get; set; }
    public string? SeccaoNome { get; set; }

    public int Categoria { get; set; }
    public string? Funcao { get; set; }

    public int UsuarioId { get; set; }
    public string? NomeUsuario { get; set; }
    public string? Perfil { get; set; }

    public int TotalDependencias { get; set; }

    public bool TemSecao { get; set; }

    public bool CandidatoManter { get; set; }

    public bool RequerDecisaoManual { get; set; }

    public string Motivo { get; set; } = string.Empty;
}


public class GrupoDuplicadoAnaliseDto
{
    public string Nome { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public int? IdSugeridoManter { get; set; }

    public bool RequerDecisaoManual { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public List<FuncionarioDuplicadoAnaliseDto> Funcionarios { get; set; }
        = new();
}