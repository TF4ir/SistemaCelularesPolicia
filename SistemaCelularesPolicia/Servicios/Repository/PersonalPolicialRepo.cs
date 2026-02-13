using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Recursos.Data;
using SistemaCelularesPolicia.Servicios.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SistemaCelularesPolicia.Servicios.Repository
{
    public class PersonalPolicialRepo : IPersonalPolicial
    {
        private readonly SisCeluPoliC _context;

        public PersonalPolicialRepo(SisCeluPoliC context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarPolicia(PersonalPolicial policia)
        {
            try
            {
                _context.PersonalPolicials.Add(policia);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistePolicia(string codigo, string dni, string email)
        {
            return await _context.PersonalPolicials
                .AnyAsync((PersonalPolicial p) => p.CodigoPolicial == codigo || p.Dni == dni || p.EmailInstitucional == email);
        }

        public async Task<PersonalPolicial> ObtenerPorEmailOCodigo(string input)
        {
            return await _context.PersonalPolicials
                .AsNoTracking()
                .Include(u => u.IdRolNavigation)
                    .ThenInclude(r => r.IdPermisos)
                .Where(p => (p.EmailInstitucional == input || p.CodigoPolicial == input)
                            && p.Activo == true)
                .FirstOrDefaultAsync();
        }

        public async Task<PersonalPolicial> ObtenerPorId(int id)
        {
            return await _context.PersonalPolicials
                .AsNoTracking()
                .Include(u => u.IdRolNavigation)
                    .ThenInclude(r => r.IdPermisos)
                .FirstOrDefaultAsync(p => p.IdPolicial == id);
        }

        public async Task<bool> Activar2fa(int idPolicial, string secretKey, string codigosRespaldo)
        {
            var policia = await _context.PersonalPolicials.FindAsync(idPolicial);
            if (policia == null) return false;

            policia.SecretKey2fa = secretKey;
            policia.CodigosRespaldo2fa = codigosRespaldo;
            policia.DosFactoresActivo = true; // Aquí activamos el candado
            policia.FechaConfiguracion2fa = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ActualizarCodigosRespaldo(int idPolicial, string nuevosCodigos)
        {
            var policia = await _context.PersonalPolicials.FindAsync(idPolicial);
            if (policia != null)
            {
                policia.CodigosRespaldo2fa = nuevosCodigos;
                policia.FechaUltimoCodigo = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RegistrarUsoCodigo2fa(int idPolicial, string codigoUsado)
        {
            var policia = await _context.PersonalPolicials.FindAsync(idPolicial);
            if (policia != null)
            {
                policia.UltimoCodigoUsado = codigoUsado;
                policia.FechaUltimoCodigo = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ActualizarPolicia(PersonalPolicial policia)
        {
            try
            {
                _context.PersonalPolicials.Update(policia);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
