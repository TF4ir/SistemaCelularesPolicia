namespace SistemaCelularesPolicia.Models.ViewModels
{
    public class RolViewModel
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; }
        public string Descripcion { get; set; }

        // La lista de checkboxes
        public List<PermisoCheck> Permisos { get; set; } = new List<PermisoCheck>();
    }

    public class PermisoCheck
    {
        public int IdPermiso { get; set; }
        public string Nombre { get; set; }
        public string Modulo { get; set; }
        public bool Seleccionado { get; set; }
    }
}