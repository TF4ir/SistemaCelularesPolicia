using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaCelularesPolicia.Models;

public partial class Celular
{
    public int IdCelular { get; set; }

    public string Imei { get; set; } = null!;

    public string? Imei2 { get; set; }

    public string Marca { get; set; } = null!;

    public string Modelo { get; set; } = null!;

    [StringLength(50)]
    public string? Color { get; set; }

    public string? Descripcion { get; set; }

    public string Situacion { get; set; } = null!;

    public DateTime FechaIncautacion { get; set; }

    public string? LugarIncautacion { get; set; }

    public string? CoordenadasIncautacion { get; set; }

    public int IdPolicialRegistro { get; set; }

    public int IdFiscalia { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual ICollection<EvidenciaCelular> EvidenciaCelulars { get; set; } = new List<EvidenciaCelular>();

    public virtual ICollection<HistoricoSituacionCelular> HistoricoSituacionCelulars { get; set; } = new List<HistoricoSituacionCelular>();

    public virtual Fiscalium IdFiscaliaNavigation { get; set; } = null!;

    public virtual PersonalPolicial IdPolicialRegistroNavigation { get; set; } = null!;
}
