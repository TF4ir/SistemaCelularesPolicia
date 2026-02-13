using System;
using System.Collections.Generic;

namespace SistemaCelularesPolicia.Models;

public partial class Permiso
{
    public int IdPermiso { get; set; }

    public string NombrePermiso { get; set; } = null!;

    public string Modulo { get; set; } = null!;

    public virtual ICollection<Role> IdRols { get; set; } = new List<Role>();
}
