namespace supai_mp.Services
{
    public interface IAutorizacaoInstitucionalService
    {
        Task<bool> TemPermissaoAsync(
            int usuarioId,
            string codigoPermissao,
            int unidadeInstitucionalId);

        Task<bool> TemPermissaoAsync(
            int usuarioId,
            string codigoPermissao,
            string siglaUnidade);

        Task<List<int>> ObterUnidadesAutorizadasAsync(
            int usuarioId,
            string codigoPermissao);
    }
}