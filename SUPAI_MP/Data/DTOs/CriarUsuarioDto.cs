namespace supai_mp.Models.DTOs
{
    public class CriarUsuarioDto
    {
        public string NomeUsuario { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty;

        public string Perfil { get; set; } = "Funcionario";

        public int FuncionarioId { get; set; }
    }
}
