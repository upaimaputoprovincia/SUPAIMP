using Microsoft.EntityFrameworkCore;
using supai_mp.Data;

namespace supai_mp.Services
{
    public class AutorizacaoInstitucionalService
        : IAutorizacaoInstitucionalService
    {
        private readonly ApplicationDbContext _context;

        public AutorizacaoInstitucionalService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // VERIFICAR PERMISSÃO POR ID DA UNIDADE
        // ============================================================

        public async Task<bool> TemPermissaoAsync(
            int usuarioId,
            string codigoPermissao,
            int unidadeInstitucionalId)
        {
            if (usuarioId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(codigoPermissao))
                return false;

            return await _context.PermissoesAcesso
                .AsNoTracking()
                .Include(p => p.TipoPermissao)
                .AnyAsync(p =>
                    p.UsuarioId == usuarioId &&
                    p.UnidadeInstitucionalId == unidadeInstitucionalId &&
                    p.Ativo &&
                    p.TipoPermissao.Ativo &&
                    p.TipoPermissao.Codigo == codigoPermissao &&
                    (
                        p.DataExpiracao == null ||
                        p.DataExpiracao >= DateTime.Now
                    ));
        }

        // ============================================================
        // VERIFICAR PERMISSÃO PELA SIGLA DA UNIDADE
        // ============================================================

        public async Task<bool> TemPermissaoAsync(
            int usuarioId,
            string codigoPermissao,
            string siglaUnidade)
        {
            if (usuarioId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(codigoPermissao))
                return false;

            if (string.IsNullOrWhiteSpace(siglaUnidade))
                return false;

            return await _context.PermissoesAcesso
                .AsNoTracking()
                .Include(p => p.TipoPermissao)
                .Include(p => p.UnidadeInstitucional)
                .AnyAsync(p =>
                    p.UsuarioId == usuarioId &&
                    p.UnidadeInstitucional.Sigla == siglaUnidade &&
                    p.Ativo &&
                    p.TipoPermissao.Ativo &&
                    p.TipoPermissao.Codigo == codigoPermissao &&
                    (
                        p.DataExpiracao == null ||
                        p.DataExpiracao >= DateTime.Now
                    ));
        }

        // ============================================================
        // OBTER TODAS AS UNIDADES AUTORIZADAS
        // ============================================================

        public async Task<List<int>> ObterUnidadesAutorizadasAsync(
            int usuarioId,
            string codigoPermissao)
        {
            if (usuarioId <= 0)
                return new List<int>();

            if (string.IsNullOrWhiteSpace(codigoPermissao))
                return new List<int>();

            return await _context.PermissoesAcesso
                .AsNoTracking()
                .Include(p => p.TipoPermissao)
                .Where(p =>
                    p.UsuarioId == usuarioId &&
                    p.Ativo &&
                    p.TipoPermissao.Ativo &&
                    p.TipoPermissao.Codigo == codigoPermissao &&
                    (
                        p.DataExpiracao == null ||
                        p.DataExpiracao >= DateTime.Now
                    ))
                .Select(p => p.UnidadeInstitucionalId)
                .Distinct()
                .ToListAsync();
        }
    }
}