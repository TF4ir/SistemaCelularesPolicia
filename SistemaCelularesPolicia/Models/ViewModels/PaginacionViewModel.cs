namespace SistemaCelularesPolicia.Models.ViewModels
{
    public class PaginacionViewModel
    {
        public List<Celular> Celulares { get; set; } = new List<Celular>();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }

        // Variables para mantener el estado de los filtros en la vista
        public string BusquedaActual { get; set; } = string.Empty;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public List<string> SituacionesSeleccionadas { get; set; } = new List<string>();
        public int? IdDependenciaFiltro { get; set; }

        public bool HasPreviousPage => PaginaActual > 1;
        public bool HasNextPage => PaginaActual < TotalPaginas;
    }
}