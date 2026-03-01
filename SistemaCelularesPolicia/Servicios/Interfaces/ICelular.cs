using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Models.ViewModels;

namespace SistemaCelularesPolicia.Servicios.Interfaces
{
    public interface ICelular
    {
        // Métodos para el Público
        Task<Celular?> BuscarPorIMEI(string imei);
        Task<Celular?> ObtenerPorId(int id);
        Task RegistrarConsulta(ConsultaPublico consulta);

        // Métodos para la Policía
        Task<PaginacionViewModel> ObtenerListadoPaginado(int pagina, int cantidadPorPagina, string busqueda, DateTime? fechaInicio, DateTime? fechaFin, List<string> situaciones, int? idDependencia);
        Task<bool> RegistrarIncautacion(Celular celular);
        Task<List<Celular>> ObtenerUltimosRegistros(int cantidad);
        Task<List<Fiscalium>> BuscarFiscalias(string term);
        Task<bool> CorregirDatosBasicos(Celular celular);
        Task<bool> CambiarSituacion(int idCelular, string nuevaSituacion, string justificacion, int idPolicia);

        // Dashboard
        Task<DashboardViewModel> ObtenerDatosDashboard(int? idDependencia);
    }
}
