using System;
using System.Collections.Generic;

namespace SistemaCelularesPolicia.Models;

public partial class Fiscalium
{
    public int IdFiscalia { get; set; }

    public string CodigoFiscalia { get; set; } = null!;

    public string NombreFiscalia { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Distrito { get; set; }

    public string? Provincia { get; set; }

    public string? Departamento { get; set; }

    public bool? Activa { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Celular> Celulars { get; set; } = new List<Celular>();
}
