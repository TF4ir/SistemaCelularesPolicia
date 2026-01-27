using System.ComponentModel.DataAnnotations;

namespace SistemaCelularesPolicia.Models.ViewModels
{
    public class SolicitarRecuperacionViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}