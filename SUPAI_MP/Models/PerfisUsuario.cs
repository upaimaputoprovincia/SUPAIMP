namespace supai_mp.Models
{
    public static class PerfisUsuario
    {
        // ============================================================
        // PERFIS DO SISTEMA
        // ============================================================

        public const string Administrador = "Administrador";

        public const string Gestor = "Gestor";

        public const string Supervisor = "Supervisor";

        public const string Operador = "Operador";

        public const string Consulta = "Consulta";

        public const string Funcionario = "Funcionario";

        // ============================================================
        // PERFIS TÉCNICOS
        // ============================================================

        /// <summary>
        /// Técnico responsável pelas operações da
        /// Secção de Segurança Pessoal.
        /// </summary>
        public const string TecnicoSP = "TecnicoSP";

        /// <summary>
        /// Técnico responsável pelas operações da
        /// Secção de Protecção de Objectos.
        /// </summary>
        public const string TecnicoPO = "TecnicoPO";


        // ============================================================
        // TODOS OS PERFIS
        // ============================================================

        public static readonly string[] Todos =
        {
            Administrador,
            Gestor,
            Supervisor,
            Operador,
            Consulta,
            Funcionario,
            TecnicoSP,
            TecnicoPO
        };


        // ============================================================
        // VALIDAR PERFIL
        // ============================================================

        public static bool EhValido(string perfil)
        {
            return Todos.Contains(
                perfil,
                StringComparer.OrdinalIgnoreCase);
        }
    }
}