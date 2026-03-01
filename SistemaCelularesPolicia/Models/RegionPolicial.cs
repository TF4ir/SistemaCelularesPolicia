using System;
using System.Collections.Generic;

namespace SistemaCelularesPolicia.Models;

public partial class RegionPolicial
{
    public int IdRegion { get; set; }

    public string NombreRegion { get; set; } = null!;

    public bool? Activa { get; set; }

    public virtual ICollection<Dependencia> Dependencia { get; set; } = new List<Dependencia>();

    public virtual ICollection<DivisionPolicial> DivisionPolicials { get; set; } = new List<DivisionPolicial>();
}
