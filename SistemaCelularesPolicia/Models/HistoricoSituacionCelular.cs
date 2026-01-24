using System;
using System.Collections.Generic;

namespace SistemaCelularesPolicia.Models;

public partial class HistoricoSituacionCelular
{
    public int IdHistorico { get; set; }

    public int IdCelular { get; set; }

    public string? SituacionAnterior { get; set; }

    public string SituacionNueva { get; set; } = null!;

    public DateTime? FechaCambio { get; set; }

    public int IdPolicialCambio { get; set; }

    public string? Observaciones { get; set; }

    public virtual Celular IdCelularNavigation { get; set; } = null!;

    public virtual PersonalPolicial IdPolicialCambioNavigation { get; set; } = null!;
}
