using System;
using System.Collections.Generic;

namespace SistemaCelularesPolicia.Models;

public partial class EvidenciaCelular
{
    public int IdEvidencia { get; set; }

    public int IdCelular { get; set; }

    public string TipoArchivo { get; set; } = null!;

    public string NombreArchivo { get; set; } = null!;

    public string RutaArchivo { get; set; } = null!;

    public string? DescripcionEvidencia { get; set; }

    public DateTime? FechaSubida { get; set; }

    public int IdPolicialSubio { get; set; }

    public virtual Celular IdCelularNavigation { get; set; } = null!;

    public virtual PersonalPolicial IdPolicialSubioNavigation { get; set; } = null!;
}
