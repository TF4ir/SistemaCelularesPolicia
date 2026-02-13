using System.ComponentModel.DataAnnotations;

namespace SistemaCelularesPolicia.Models
{
    public class Dependencia
    {
        [Key]
        public int IdDependencia { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        public string? Siglas { get; set; }

        public string? Provincia { get; set; }

        public bool Activa { get; set; } = true;
    }
}