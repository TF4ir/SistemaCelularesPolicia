using System;
using System.Collections.Generic;

namespace SistemaCelularesPolicia.Models;

public partial class UsuarioPublico
{
    public int IdUsuarioPublico { get; set; }

    public string Dni { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string ContraseñaHash { get; set; } = null!;

    public bool? Activo { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public string? TokenVerificacion { get; set; }

    public bool? EmailVerificado { get; set; }

    public virtual ICollection<ConsultaPublico> ConsultaPublicos { get; set; } = new List<ConsultaPublico>();
}
