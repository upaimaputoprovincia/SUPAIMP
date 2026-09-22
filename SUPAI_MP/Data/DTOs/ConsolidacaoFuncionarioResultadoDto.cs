namespace supai_mp.DTOs
{
    public class ConsolidarFuncionariosDuplicadosDto
    {
        public int IdManter { get; set; }
        public int IdEliminar { get; set; }
        public bool ConfirmacaoManual { get; set; }
    }

    public class ConsolidacaoFuncionarioResultadoDto
    {
        public bool Sucesso { get; set; }

        public int IdMantido { get; set; }

        public int IdEliminado { get; set; }

        public string NomeCompleto { get; set; } = string.Empty;

        public List<string> DependenciasTransferidas { get; set; }
            = new();

        public List<string> DependenciasNaoTransferidas { get; set; }
            = new();

        public string Mensagem { get; set; } = string.Empty;

        
    }
}