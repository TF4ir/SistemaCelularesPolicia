using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Servicios.Interfaces;
using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Recursos.Data;

namespace SistemaCelularesPolicia.Servicios.Repository
{
    public class UsuarioPublicoRepo: IUsuarioPublico
    {
        private readonly SisCeluPoliC _context;

        public UsuarioPublicoRepo(SisCeluPoliC context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarUsuario(UsuarioPublico usuario)
        {
            try
            {
                _context.UsuarioPublicos.Add(usuario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExisteUsuario(string email, string dni)
        {
            return await _context.UsuarioPublicos
                .AnyAsync(u => u.Email == email || u.Dni == dni);
        }

        public async Task<UsuarioPublico> ObtenerPorEmail(string email)
        {
            return await _context.UsuarioPublicos
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<bool> ActualizarUsuario(UsuarioPublico usuario)
        {
            try
            {
                _context.UsuarioPublicos.Update(usuario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
