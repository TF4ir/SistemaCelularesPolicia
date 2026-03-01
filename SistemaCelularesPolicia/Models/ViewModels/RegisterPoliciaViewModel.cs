using System.ComponentModel.DataAnnotations;

namespace SistemaCelularesPolicia.Models.ViewModels
{
    public class RegisterPoliciaViewModel
    {
        [Display(Name = "Código Policial (CIP)")]
        [Required(ErrorMessage = "El CIP es obligatorio.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El CIP debe tener exactamente 8 dígitos.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El CIP solo debe contener números.")]
        public string CodigoPolicial { get; set; } = null!;

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El DNI solo debe contener números.")]
        public string Dni { get; set; } = null!;

        [Required(ErrorMessage = "Ingrese sus nombres.")]
        public string Nombres { get; set; } = null!;

        [Required(ErrorMessage = "Ingrese sus apellidos.")]
        public string Apellidos { get; set; } = null!;

        [Required]
        public string RangoGrado { get; set; } = null!;

        [Required]
        public string UnidadDependencia { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar la unidad o comisaría.")]
        public int IdDependencia { get; set; }

        [Required(ErrorMessage = "El correo institucional es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        // ESTA ES LA LÍNEA MÁGICA:
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(policia\.gob\.pe|pnp\.gob\.pe)$",
        ErrorMessage = "Solo se permiten correos institucionales (@policia.gob.pe o @pnp.gob.pe).")]
        public string EmailInstitucional { get; set; } = null!;

        // CAMPOS DE CONTRASEÑA (Solo existen en el ViewModel, no en la BD)
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "El número de contacto es obligatorio.")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de contacto debe tener 9 dígitos.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El número de contacto solo debe contener números.")]
        public string TelefonoContacto { get; set;} = null!;
    }
}