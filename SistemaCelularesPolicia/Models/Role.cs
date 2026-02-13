using System;
using System.Collections.Generic;

namespace SistemaCelularesPolicia.Models;

public partial class Role
{
    public int IdRol { get; set; }

    public string NombreRol { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<PersonalPolicial> PersonalPolicials { get; set; } = new List<PersonalPolicial>();

    public virtual ICollection<Permiso> IdPermisos { get; set; } = new List<Permiso>();
}
