using System.ComponentModel.DataAnnotations;

namespace SistemaCelularesPolicia.Models.ViewModels
{
    public class RestablecerPasswordViewModel
    {
        [Required]
        public string Email { get; set; } = null!; // Oculto

        [Required(ErrorMessage = "El código es obligatorio.")]
        public string Codigo { get; set; } = null!;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        public string NuevaPassword { get; set; } = null!;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = null!;
    }
}