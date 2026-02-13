using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaCelularesPolicia.Models;

public partial class PersonalPolicial
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPolicial { get; set; }

    public string CodigoPolicial { get; set; } = null!;

    public string Dni { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string RangoGrado { get; set; } = null!;

    public string UnidadDependencia { get; set; } = null!;

    public string? DepartamentoPolicial { get; set; }

    public string? EmailInstitucional { get; set; } = null!;

    public string? TelefonoContacto { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public string? SecretKey2fa { get; set; }

    public DateTime? FechaConfiguracion2fa { get; set; }

    public bool? DosFactoresActivo { get; set; }

    public string? CodigosRespaldo2fa { get; set; }

    public string? UltimoCodigoUsado { get; set; }

    public DateTime? FechaUltimoCodigo { get; set; }

    public int? IntentosFallidos2fa { get; set; }

    public DateTime? BloqueadoHasta { get; set; }

    public string? MetodoAlternativo { get; set; }

    public bool? RecordarDispositivo { get; set; }

    public string? TokenRecordar { get; set; }

    public string? ContrasenaHash { get; set; } = null!;

    public string? CodVerificacionEmail { get; set; }

    public DateTime? FechaExpiracionCod { get; set; }

    public bool? EmailVerificado { get; set; }

    public int? IdRol { get; set; }

    public virtual ICollection<Celular> Celulars { get; set; } = new List<Celular>();

    public virtual ICollection<EvidenciaCelular> EvidenciaCelulars { get; set; } = new List<EvidenciaCelular>();

    public virtual ICollection<HistoricoSituacionCelular> HistoricoSituacionCelulars { get; set; } = new List<HistoricoSituacionCelular>();

    public virtual Role? IdRolNavigation { get; set; }
}
