namespace SistemaCelularesPolicia.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalRegistrados { get; set; }
        public int TotalIncautados { get; set; }
        public int TotalRecuperados { get; set; }
        public int TotalDevueltos { get; set; }

        // 2. Datos para Gráficos (Listas simplificadas)
        public List<string> LabelsSituacion { get; set; } = new List<string>();
        public List<int> DataSituacion { get; set; } = new List<int>();

        public List<string> LabelsMarcas { get; set; } = new List<string>();
        public List<int> DataMarcas { get; set; } = new List<int>();

        // 3. Tabla de Últimos Registros
        public List<Celular> UltimosRegistros { get; set; } = new List<Celular>();
    }
}