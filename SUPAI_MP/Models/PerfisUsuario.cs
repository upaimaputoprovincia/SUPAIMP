namespace supai_mp.Models
{
    public static class PerfisUsuario
    {
        public const string Administrador = "Administrador";
        public const string Gestor = "Gestor";
        public const string Supervisor = "Supervisor";
        public const string Operador = "Operador";
        public const string Consulta = "Consulta";
        public const string Funcionario = "Funcionario";

        public static readonly string[] Todos =
        {
            Administrador,
            Gestor,
            Supervisor,
            Operador,
            Consulta,
            Funcionario
        };

        public static bool EhValido(string perfil)
        {
            return Todos.Contains(
                perfil,
                StringComparer.OrdinalIgnoreCase);
        }
    }
}