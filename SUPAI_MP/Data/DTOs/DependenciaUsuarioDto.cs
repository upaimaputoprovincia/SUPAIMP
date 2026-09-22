namespace supai_mp.DTOs
{
    public class DependenciaUsuarioDto
    {
        public int UsuarioId { get; set; }

        public string Tabela { get; set; } = string.Empty;

        public string Coluna { get; set; } = string.Empty;

        public int Quantidade { get; set; }
    }

    public class UsuarioDuplicadoDependenciasDto
    {
        public int FuncionarioId { get; set; }

        public string NomeCompleto { get; set; } = string.Empty;

        public int UsuarioId { get; set; }

        public string? NomeUsuario { get; set; }

        public string? Perfil { get; set; }

        public int TotalDependencias { get; set; }

        public List<DependenciaUsuarioDto> Dependencias { get; set; }
            = new();
    }
}