using SistemaCelularesPolicia.Models;

namespace SistemaCelularesPolicia.Servicios.Interfaces
{
    public interface IPersonalPolicial
    {
        Task<bool> RegistrarPolicia(PersonalPolicial policia);
        // Validar que no se repita el Código, DNI o Email
        Task<bool> ExistePolicia(string codigo, string dni, string email);
        Task<PersonalPolicial> ObtenerPorEmailOCodigo(string input);
        Task<bool> Activar2fa(int idPolicial, string secretKey, string codigosRespaldo);
        Task<PersonalPolicial> ObtenerPorId(int id);
        Task ActualizarCodigosRespaldo(int idPolicial, string nuevosCodigos);
        Task RegistrarUsoCodigo2fa(int idPolicial, string codigoUsado);
    }
}
