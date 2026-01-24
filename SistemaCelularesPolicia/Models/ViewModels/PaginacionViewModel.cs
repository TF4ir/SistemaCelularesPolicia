namespace SistemaCelularesPolicia.Models.ViewModels
{
    public class PaginacionViewModel
    {
        public List<Celular> Celulares { get; set; } = new List<Celular>(); // Los registros de ESTA página
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }

        public string BusquedaActual { get; set; } = string.Empty;

        public bool HasPreviousPage => PaginaActual > 1;
        public bool HasNextPage => PaginaActual < TotalPaginas;
    }
}