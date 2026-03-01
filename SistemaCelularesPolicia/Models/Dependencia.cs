using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaCelularesPolicia.Models;

public partial class Dependencia
{
    [Key]
    public int IdDependencia { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = null!;

    public string? Siglas { get; set; }

    public string? Provincia { get; set; }

    public bool? Activa { get; set; }

    public int? IdRegion { get; set; }

    public int? IdDivision { get; set; }

    public virtual DivisionPolicial? IdDivisionNavigation { get; set; }

    public virtual RegionPolicial? IdRegionNavigation { get; set; }

    public virtual ICollection<PersonalPolicial> PersonalPolicials { get; set; } = new List<PersonalPolicial>();
}
