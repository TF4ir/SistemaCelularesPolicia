using System;
using System.Collections.Generic;

namespace SistemaCelularesPolicia.Models;

public partial class PersonalPolicial
{
    public int IdPolicial { get; set; }

    public string CodigoPolicial { get; set; } = null!;

    public string Dni { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string RangoGrado { get; set; } = null!;

    public string UnidadDependencia { get; set; } = null!;

    public string? DepartamentoPolicial { get; set; }

    public string? EmailInstitucional { get; set; }

    public string? TelefonoContacto { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public virtual ICollection<Celular> Celulars { get; set; } = new List<Celular>();

    public virtual ICollection<EvidenciaCelular> EvidenciaCelulars { get; set; } = new List<EvidenciaCelular>();

    public virtual ICollection<HistoricoSituacionCelular> HistoricoSituacionCelulars { get; set; } = new List<HistoricoSituacionCelular>();
}
