namespace supai_mp.Models.DTOs
{
    public class FuncionarioPesquisaDto
    {
        public string? Nome { get; set; }

        public string? Nip { get; set; }

        public string? Bi { get; set; }

        public string? Nuit { get; set; }

        public string? Contacto { get; set; }

        public Categoria? Categoria { get; set; }

        public string? Funcao { get; set; }

        public string? LocalTrabalho { get; set; }

        public EstadoFuncionario? EstadoFuncionario { get; set; }
    }
}