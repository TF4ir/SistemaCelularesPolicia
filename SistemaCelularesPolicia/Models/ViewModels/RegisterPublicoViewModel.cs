using System.ComponentModel.DataAnnotations;

namespace SistemaCelularesPolicia.Models.ViewModels
{
    public class RegisterPublicoViewModel
    {
        [Display(Name = "DNI")]
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El DNI solo debe contener números.")]
        public string Dni { get; set; } = null!;

        [Required(ErrorMessage = "Ingrese sus nombres.")]
        public string Nombres { get; set; } = null!;

        [Required(ErrorMessage = "Ingrese sus apellidos.")]
        public string Apellidos { get; set; } = null!;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El número de telefono es obligatorio.")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de telefono debe tener 9 dígitos.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El número de telefono solo debe contener números.")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateOnly? FechaNacimiento { get; set; }

        // --- CONTRASEÑAS (Solo en ViewModel) ---
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirme la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}