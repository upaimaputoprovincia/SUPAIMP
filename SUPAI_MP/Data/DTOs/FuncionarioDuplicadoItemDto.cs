namespace supai_mp.DTOs
{
    public class FuncionarioDuplicadoItemDto
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string Nip { get; set; } = string.Empty;
        public int Categoria { get; set; }
        public string? Funcao { get; set; }
        public int? SeccaoId { get; set; }
        public string? SeccaoNome { get; set; }
        public int Estado { get; set; }
    }

    public class GrupoFuncionarioDuplicadoDto
    {
        public string Nome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public List<FuncionarioDuplicadoItemDto> Funcionarios { get; set; } = new();
    }

    public class DependenciaFuncionarioDto
    {
        public string Entidade { get; set; } = string.Empty;
        public string Tabela { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }

    public class FuncionarioDuplicadoDetalheDto
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string Nip { get; set; } = string.Empty;
        public int Categoria { get; set; }
        public string? Funcao { get; set; }
        public int? SeccaoId { get; set; }
        public string? SeccaoNome { get; set; }
        public int Estado { get; set; }

        public int TotalDependencias { get; set; }

        public List<DependenciaFuncionarioDto> Dependencias { get; set; } = new();
    }
}