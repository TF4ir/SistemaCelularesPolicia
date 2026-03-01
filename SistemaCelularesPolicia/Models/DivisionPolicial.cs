using System;
using System.Collections.Generic;

namespace SistemaCelularesPolicia.Models;

public partial class DivisionPolicial
{
    public int IdDivision { get; set; }

    public int IdRegion { get; set; }

    public string NombreDivision { get; set; } = null!;

    public bool? Activa { get; set; }

    public virtual ICollection<Dependencia> Dependencia { get; set; } = new List<Dependencia>();

    public virtual RegionPolicial IdRegionNavigation { get; set; } = null!;
}
