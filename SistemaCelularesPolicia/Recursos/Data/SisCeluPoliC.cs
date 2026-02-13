using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Models;

namespace SistemaCelularesPolicia.Recursos.Data;

public partial class SisCeluPoliC : DbContext
{
    public SisCeluPoliC()
    {
    }

    public SisCeluPoliC(DbContextOptions<SisCeluPoliC> options)
        : base(options)
    {
    }

    public virtual DbSet<Celular> Celulars { get; set; }

    public virtual DbSet<ConsultaPublico> ConsultaPublicos { get; set; }

    public virtual DbSet<EvidenciaCelular> EvidenciaCelulars { get; set; }

    public virtual DbSet<Fiscalium> Fiscalia { get; set; }

    public virtual DbSet<HistoricoSituacionCelular> HistoricoSituacionCelulars { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<PersonalPolicial> PersonalPolicials { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Dependencia> Dependencias { get; set; }

    public virtual DbSet<UsuarioPublico> UsuarioPublicos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Celular>(entity =>
        {
            entity.HasKey(e => e.IdCelular).HasName("PK__celulare__3320C56C65A8AC95");

            entity.ToTable("celular");

            entity.HasIndex(e => e.FechaIncautacion, "IX_celulares_fecha_incautacion");

            entity.HasIndex(e => e.Imei, "IX_celulares_imei");

            entity.HasIndex(e => e.Situacion, "IX_celulares_situacion");

            entity.HasIndex(e => e.Imei, "UQ__celulare__9BF7BEB8BC083C0F").IsUnique();

            entity.Property(e => e.IdCelular).HasColumnName("id_celular");
            entity.Property(e => e.Color)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("color");
            entity.Property(e => e.CoordenadasIncautacion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("coordenadas_incautacion");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaIncautacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_incautacion");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdFiscalia).HasColumnName("id_fiscalia");
            entity.Property(e => e.IdPolicialRegistro).HasColumnName("id_policial_registro");
            entity.Property(e => e.Imei)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("imei");
            entity.Property(e => e.Imei2)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("imei2");
            entity.Property(e => e.LugarIncautacion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("lugar_incautacion");
            entity.Property(e => e.Marca)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("marca");
            entity.Property(e => e.Modelo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modelo");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.Situacion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("situacion");

            entity.HasOne(d => d.IdFiscaliaNavigation).WithMany(p => p.Celulars)
                .HasForeignKey(d => d.IdFiscalia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__celulares__id_fi__4E88ABD4");

            entity.HasOne(d => d.IdPolicialRegistroNavigation).WithMany(p => p.Celulars)
                .HasForeignKey(d => d.IdPolicialRegistro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__celulares__id_po__4D94879B");
        });

        modelBuilder.Entity<ConsultaPublico>(entity =>
        {
            entity.HasKey(e => e.IdConsulta).HasName("PK__consulta__6F53588BB16EF6EC");

            entity.ToTable("consulta_publico");

            entity.HasIndex(e => e.FechaConsulta, "IX_consultas_fecha");

            entity.HasIndex(e => e.Resultado, "IX_consultas_resultado");

            entity.HasIndex(e => e.IdUsuarioPublico, "IX_consultas_usuario");

            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.FechaConsulta)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_consulta");
            entity.Property(e => e.IdUsuarioPublico).HasColumnName("id_usuario_publico");
            entity.Property(e => e.ImeiConsultado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("imei_consultado");
            entity.Property(e => e.IpConsulta)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("ip_consulta");
            entity.Property(e => e.Resultado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("resultado");
            entity.Property(e => e.UserAgent)
                .HasColumnType("text")
                .HasColumnName("user_agent");

            entity.HasOne(d => d.IdUsuarioPublicoNavigation).WithMany(p => p.ConsultaPublicos)
                .HasForeignKey(d => d.IdUsuarioPublico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__consultas__id_us__534D60F1");
        });

        modelBuilder.Entity<EvidenciaCelular>(entity =>
        {
            entity.HasKey(e => e.IdEvidencia).HasName("PK__evidenci__62875FB9A2FC053B");

            entity.ToTable("evidencia_celular");

            entity.HasIndex(e => e.IdCelular, "IX_evidencias_celular");

            entity.Property(e => e.IdEvidencia).HasColumnName("id_evidencia");
            entity.Property(e => e.DescripcionEvidencia)
                .HasColumnType("text")
                .HasColumnName("descripcion_evidencia");
            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_subida");
            entity.Property(e => e.IdCelular).HasColumnName("id_celular");
            entity.Property(e => e.IdPolicialSubio).HasColumnName("id_policial_subio");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.RutaArchivo)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("ruta_archivo");
            entity.Property(e => e.TipoArchivo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("tipo_archivo");

            entity.HasOne(d => d.IdCelularNavigation).WithMany(p => p.EvidenciaCelulars)
                .HasForeignKey(d => d.IdCelular)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__evidencia__id_ce__5EBF139D");

            entity.HasOne(d => d.IdPolicialSubioNavigation).WithMany(p => p.EvidenciaCelulars)
                .HasForeignKey(d => d.IdPolicialSubio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__evidencia__id_po__5FB337D6");
        });

        modelBuilder.Entity<Fiscalium>(entity =>
        {
            entity.HasKey(e => e.IdFiscalia).HasName("PK__fiscalia__222CB26120FDB19D");

            entity.ToTable("fiscalia");

            entity.HasIndex(e => e.Activa, "IX_fiscalias_activa");

            entity.HasIndex(e => e.CodigoFiscalia, "UQ__fiscalia__C15340C0806E90A5").IsUnique();

            entity.Property(e => e.IdFiscalia).HasColumnName("id_fiscalia");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.CodigoFiscalia)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("codigo_fiscalia");
            entity.Property(e => e.Departamento)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("departamento");
            entity.Property(e => e.Direccion)
                .HasColumnType("text")
                .HasColumnName("direccion");
            entity.Property(e => e.Distrito)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("distrito");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.NombreFiscalia)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre_fiscalia");
            entity.Property(e => e.Provincia)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("provincia");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<HistoricoSituacionCelular>(entity =>
        {
            entity.HasKey(e => e.IdHistorico).HasName("PK__historic__76E62AC3BC805891");

            entity.ToTable("historico_situacion_celular");

            entity.HasIndex(e => e.IdCelular, "IX_historico_celular");

            entity.HasIndex(e => e.FechaCambio, "IX_historico_fecha");

            entity.Property(e => e.IdHistorico).HasColumnName("id_historico");
            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_cambio");
            entity.Property(e => e.IdCelular).HasColumnName("id_celular");
            entity.Property(e => e.IdPolicialCambio).HasColumnName("id_policial_cambio");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.SituacionAnterior)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("situacion_anterior");
            entity.Property(e => e.SituacionNueva)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("situacion_nueva");

            entity.HasOne(d => d.IdCelularNavigation).WithMany(p => p.HistoricoSituacionCelulars)
                .HasForeignKey(d => d.IdCelular)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__historico__id_ce__59063A47");

            entity.HasOne(d => d.IdPolicialCambioNavigation).WithMany(p => p.HistoricoSituacionCelulars)
                .HasForeignKey(d => d.IdPolicialCambio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__historico__id_po__59FA5E80");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.IdPermiso).HasName("PK__Permisos__0D626EC85F67A4B0");

            entity.HasIndex(e => e.NombrePermiso, "UQ__Permisos__BA19B18D24AEFD3F").IsUnique();

            entity.Property(e => e.Modulo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NombrePermiso)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PersonalPolicial>(entity =>
        {
            entity.HasKey(e => e.IdPolicial).HasName("PK__personal__390DDAAC1ECF65EB");

            entity.ToTable("personal_policial");

            entity.HasIndex(e => e.DosFactoresActivo, "IX_personal_2fa_activo");

            entity.HasIndex(e => e.Activo, "IX_personal_activo");

            entity.HasIndex(e => e.BloqueadoHasta, "IX_personal_bloqueado");

            entity.HasIndex(e => e.CodigoPolicial, "UQ__personal__86AB43652F1FF89F").IsUnique();

            entity.HasIndex(e => e.EmailInstitucional, "UQ__personal__A3E90BD630EF438C").IsUnique();

            entity.HasIndex(e => e.Dni, "UQ__personal__D87608A7F38AE5FD").IsUnique();

            entity.Property(e => e.IdPolicial).HasColumnName("id_policial");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellidos");
            entity.Property(e => e.BloqueadoHasta)
                .HasColumnType("datetime")
                .HasColumnName("bloqueado_hasta");
            entity.Property(e => e.CodVerificacionEmail)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("cod_verificacion_email");
            entity.Property(e => e.CodigoPolicial)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("codigo_policial");
            entity.Property(e => e.CodigosRespaldo2fa)
                .IsUnicode(false)
                .HasColumnName("codigos_respaldo_2fa");
            entity.Property(e => e.ContrasenaHash)
                .HasMaxLength(255)
                .HasColumnName("contrasena_hash");
            entity.Property(e => e.DepartamentoPolicial)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("departamento_policial");
            entity.Property(e => e.Dni)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("dni");
            entity.Property(e => e.DosFactoresActivo)
                .HasDefaultValue(false)
                .HasColumnName("dos_factores_activo");
            entity.Property(e => e.EmailInstitucional)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email_institucional");
            entity.Property(e => e.EmailVerificado)
                .HasDefaultValue(false)
                .HasColumnName("email_verificado");
            entity.Property(e => e.FechaConfiguracion2fa)
                .HasColumnType("datetime")
                .HasColumnName("fecha_configuracion_2fa");
            entity.Property(e => e.FechaExpiracionCod)
                .HasColumnType("datetime")
                .HasColumnName("fecha_expiracion_cod");
            entity.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FechaUltimoCodigo)
                .HasColumnType("datetime")
                .HasColumnName("fecha_ultimo_codigo");
            entity.Property(e => e.IntentosFallidos2fa)
                .HasDefaultValue(0)
                .HasColumnName("intentos_fallidos_2fa");
            entity.Property(e => e.MetodoAlternativo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("metodo_alternativo");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombres");
            entity.Property(e => e.RangoGrado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("rango_grado");
            entity.Property(e => e.RecordarDispositivo)
                .HasDefaultValue(false)
                .HasColumnName("recordar_dispositivo");
            entity.Property(e => e.SecretKey2fa)
                .HasMaxLength(32)
                .IsUnicode(false)
                .HasColumnName("secret_key_2fa");
            entity.Property(e => e.TelefonoContacto)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono_contacto");
            entity.Property(e => e.TokenRecordar)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("token_recordar");
            entity.Property(e => e.UltimoAcceso)
                .HasColumnType("datetime")
                .HasColumnName("ultimo_acceso");
            entity.Property(e => e.UltimoCodigoUsado)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("ultimo_codigo_usado");
            entity.Property(e => e.UnidadDependencia)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("unidad_dependencia");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.PersonalPolicials)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK_Personal_Rol");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__2A49584C402F1A49");

            entity.HasIndex(e => e.NombreRol, "UQ__Roles__4F0B537F784F62C6").IsUnique();

            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasMany(d => d.IdPermisos).WithMany(p => p.IdRols)
                .UsingEntity<Dictionary<string, object>>(
                    "RolPermiso",
                    r => r.HasOne<Permiso>().WithMany()
                        .HasForeignKey("IdPermiso")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Rol_Permi__IdPer__42E1EEFE"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("IdRol")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Rol_Permi__IdRol__41EDCAC5"),
                    j =>
                    {
                        j.HasKey("IdRol", "IdPermiso").HasName("PK__Rol_Perm__BA9F7EA0C6D1F1C9");
                        j.ToTable("Rol_Permiso");
                    });
        });

        modelBuilder.Entity<UsuarioPublico>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioPublico).HasName("PK__usuarios__B095121E3C51EDE0");

            entity.ToTable("usuario_publico");

            entity.HasIndex(e => e.Activo, "IX_usuarios_activo");

            entity.HasIndex(e => e.Email, "IX_usuarios_email");

            entity.HasIndex(e => e.Email, "UQ__usuarios__AB6E616459D34B1C").IsUnique();

            entity.HasIndex(e => e.Dni, "UQ__usuarios__D87608A7EB3E335F").IsUnique();

            entity.Property(e => e.IdUsuarioPublico).HasColumnName("id_usuario_publico");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellidos");
            entity.Property(e => e.CodVerificacionEmail)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("cod_verificacion_email");
            entity.Property(e => e.ContraseñaHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contraseña_hash");
            entity.Property(e => e.Direccion)
                .HasColumnType("text")
                .HasColumnName("direccion");
            entity.Property(e => e.Dni)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("dni");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerificado)
                .HasDefaultValue(false)
                .HasColumnName("email_verificado");
            entity.Property(e => e.FechaExpiracionCod)
                .HasColumnType("datetime")
                .HasColumnName("fecha_expiracion_cod");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombres");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.UltimoAcceso)
                .HasColumnType("datetime")
                .HasColumnName("ultimo_acceso");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
