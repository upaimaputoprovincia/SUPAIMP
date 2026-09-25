namespace supai_mp.Models.DTOs
{
    public class EditarAcessoDto
    {
        public string NomeUsuario { get; set; } = string.Empty;

        public string? NovaSenha { get; set; }

        public string Perfil { get; set; } = "Funcionario";
    }
}
