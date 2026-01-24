using SistemaCelularesPolicia.Models;

namespace SistemaCelularesPolicia.Servicios.Interfaces
{
    public interface IUsuarioPublico
    {
        Task<bool> RegistrarUsuario(UsuarioPublico usuario);

        // Método para verificar si ya existe el correo o DNI (para evitar duplicados)
        Task<bool> ExisteUsuario(string email, string dni);

        // Método para obtener usuario por email (para el login futuro)
        Task<UsuarioPublico> ObtenerPorEmail(string email);

    }
}
