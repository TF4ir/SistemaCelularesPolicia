namespace SistemaCelularesPolicia.Servicios.Interfaces
{
    public interface IEmail
    {
        Task EnviarCorreoVerificacion(string emailDestino, string nombreUsuario, string codigo);
    }
}
