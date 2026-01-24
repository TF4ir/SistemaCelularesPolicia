using System;
using System.Collections.Generic;

namespace SistemaCelularesPolicia.Models;

public partial class ConsultaPublico
{
    public int IdConsulta { get; set; }

    public int IdUsuarioPublico { get; set; }

    public string ImeiConsultado { get; set; } = null!;

    public DateTime? FechaConsulta { get; set; }

    public string Resultado { get; set; } = null!;

    public string? IpConsulta { get; set; }

    public string? UserAgent { get; set; }

    public virtual UsuarioPublico IdUsuarioPublicoNavigation { get; set; } = null!;
}
