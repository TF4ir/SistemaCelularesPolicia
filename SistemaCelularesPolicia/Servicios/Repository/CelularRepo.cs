using Microsoft.EntityFrameworkCore;
using SistemaCelularesPolicia.Models;
using SistemaCelularesPolicia.Models.ViewModels;
using SistemaCelularesPolicia.Recursos.Data;
using SistemaCelularesPolicia.Servicios.Interfaces;

namespace SistemaCelularesPolicia.Servicios.Repository
{
    public class CelularRepo:ICelular
    {
        private readonly SisCeluPoliC _context;

        public CelularRepo(SisCeluPoliC context)
        {
            _context = context;
        }

        public async Task<Celular?> BuscarPorIMEI(string imei)
        {
            // Buscamos el celular y unimos las tablas relacionadas
            return await _context.Celulars
                .Include(c => c.IdFiscaliaNavigation)         // Datos de la Fiscalía
                .Include(c => c.IdPolicialRegistroNavigation) // Datos del Policía
                .FirstOrDefaultAsync(c => c.Imei == imei || c.Imei2 == imei);
        }

        public async Task<Celular?> ObtenerPorId(int idCelular)
        {
            return await _context.Celulars
                .Include(c => c.IdFiscaliaNavigation)
                .Include(c => c.IdPolicialRegistroNavigation)
                .FirstOrDefaultAsync(c => c.IdCelular == idCelular);
        }

        public async Task<PaginacionViewModel> ObtenerListadoPaginado(int pagina, int cantidadPorPagina, string busqueda, DateTime? fechaInicio, DateTime? fechaFin, List<string> situaciones, int? idDependencia)
        {
            // 1. Consulta base (Hacemos Include al policía para saber a qué dependencia pertenece)
            var query = _context.Celulars
                .Include(c => c.IdFiscaliaNavigation)
                .Include(c => c.IdPolicialRegistroNavigation)
                .AsQueryable();

            // 2. Filtro de Búsqueda de texto (IMEI, Marca, Modelo)
            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(c => c.Imei.Contains(busqueda) || c.Marca.Contains(busqueda) || c.Modelo.Contains(busqueda));
            }

            // 3. Filtro de Data Scoping (Dependencia / Comisaría)
            if (idDependencia.HasValue)
            {
                query = query.Where(c => c.IdPolicialRegistroNavigation.IdDependencia == idDependencia.Value);
            }

            // 4. Filtro de Fechas (Rango por Fecha de Incautación)
            if (fechaInicio.HasValue)
            {
                query = query.Where(c => c.FechaIncautacion >= fechaInicio.Value);
            }
            if (fechaFin.HasValue)
            {
                // Sumamos 1 día y restamos 1 tick para incluir hasta las 23:59:59 del día de fin
                var fin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(c => c.FechaIncautacion <= fin);
            }

            // 5. Filtro de Situación (Checkboxes)
            if (situaciones != null && situaciones.Any())
            {
                query = query.Where(c => situaciones.Contains(c.Situacion));
            }

            // 6. CONTAR EL TOTAL DE LOS FILTRADOS
            var totalRegistros = await query.CountAsync();

            // 7. Calcular páginas
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)cantidadPorPagina);

            // 8. Aplicar orden y paginación
            var registros = await query
                .OrderByDescending(c => c.FechaRegistro)
                .Skip((pagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .ToListAsync();

            return new PaginacionViewModel
            {
                Celulares = registros,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros, // Enviamos el contador a la vista
                BusquedaActual = busqueda ?? "",
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                SituacionesSeleccionadas = situaciones ?? new List<string>(),
                IdDependenciaFiltro = idDependencia
            };
        }

        public async Task RegistrarConsulta(ConsultaPublico consulta)
        {
            _context.ConsultaPublicos.Add(consulta);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RegistrarIncautacion(Celular celular)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Guardar el Celular
                _context.Celulars.Add(celular);
                await _context.SaveChangesAsync(); // Esto genera el IdCelular

                // 2. Guardar el Histórico Inicial (Trazabilidad)
                var historico = new HistoricoSituacionCelular
                {
                    IdCelular = celular.IdCelular,
                    SituacionAnterior = null,
                    SituacionNueva = celular.Situacion,
                    FechaCambio = DateTime.UtcNow,
                    IdPolicialCambio = celular.IdPolicialRegistro,
                    Observaciones = "Registro inicial del operativo."
                };

                _context.HistoricoSituacionCelulars.Add(historico);
                await _context.SaveChangesAsync();

                // Confirmar transacción
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // 4. Obtener Fiscalías para el Dropdown
        public async Task<List<Fiscalium>> BuscarFiscalias(string term)
        {
            return await _context.Fiscalia
                .Where(f => f.Activa == true && f.NombreFiscalia.Contains(term))
                .OrderBy(f => f.NombreFiscalia)
                .Take(20)
                .ToListAsync();
        }

        public async Task<List<Celular>> ObtenerUltimosRegistros(int cantidad)
        {
            return await _context.Celulars
                .OrderByDescending(c => c.FechaRegistro)
                .Take(cantidad)
                .Include(c => c.IdFiscaliaNavigation)
                .ToListAsync();
        }
        public async Task<bool> CorregirDatosBasicos(Celular celular) 
        {
            var existente = await _context.Celulars.FindAsync(celular.IdCelular);
            if (existente == null) return false;

            existente.Imei = celular.Imei;
            existente.Imei2 = celular.Imei2;
            existente.Marca = celular.Marca;
            existente.Modelo = celular.Modelo;
            existente.Color = celular.Color;
            existente.Descripcion = celular.Descripcion; // <--- Descripción física
            existente.Observaciones = celular.Observaciones; // Contexto del operativo
            existente.LugarIncautacion = celular.LugarIncautacion;
            existente.IdFiscalia = celular.IdFiscalia;

            // --- CAMPOS QUE NO SE TOCAN (Seguridad) ---
            // existente.Situacion (Se usa el otro método)
            // existente.FechaIncautacion
            // existente.IdPolicialRegistro

            existente.FechaActualizacion = DateTime.UtcNow; // Marcar cuándo se editó

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CambiarSituacion(int idCelular, string nuevaSituacion, string justificacion, int idPolicia)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var celular = await _context.Celulars.FindAsync(idCelular);

                // Si la situación es la misma, no hacemos nada
                if (celular.Situacion == nuevaSituacion) return false;

                string situacionAnterior = celular.Situacion;

                // Actualizar Celular
                celular.Situacion = nuevaSituacion;
                celular.FechaActualizacion = DateTime.UtcNow;

                // Insertar Histórico
                var historico = new HistoricoSituacionCelular
                {
                    IdCelular = celular.IdCelular,
                    SituacionAnterior = situacionAnterior,
                    SituacionNueva = nuevaSituacion,
                    FechaCambio = DateTime.UtcNow,
                    IdPolicialCambio = idPolicia,
                    Observaciones = justificacion // Ej: "Devolución al dueño según acta..."
                };

                _context.HistoricoSituacionCelulars.Add(historico);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<DashboardViewModel> ObtenerDatosDashboard(int? idDependencia)
        {
            var vm = new DashboardViewModel();

            // 1. Consulta Base Segura
            var query = _context.Celulars.AsQueryable();

            if (idDependencia.HasValue)
            {
                // Si no es admin, filtramos TODO el dashboard a su comisaría
                query = query.Where(c => c.IdPolicialRegistroNavigation.IdDependencia == idDependencia.Value);
            }

            // 2. Contadores Rápidos (Usando la query segura)
            vm.TotalRegistrados = await query.CountAsync();
            vm.TotalIncautados = await query.CountAsync(c => c.Situacion.ToUpper() == "INCAUTADO");
            vm.TotalRecuperados = await query.CountAsync(c => c.Situacion.ToUpper() == "RECUPERADO");
            vm.TotalDevueltos = await query.CountAsync(c => c.Situacion.ToUpper() == "DEVUELTO");

            // 3. Gráfico de Situación
            var datosSituacion = await query.GroupBy(c => c.Situacion)
                .Select(g => new { Situacion = g.Key, Cantidad = g.Count() }).ToListAsync();
            vm.LabelsSituacion = datosSituacion.Select(x => x.Situacion ?? "OTRO").ToList();
            vm.DataSituacion = datosSituacion.Select(x => x.Cantidad).ToList();

            // 4. Gráfico de Marcas (Top 5)
            var datosMarcas = await query.GroupBy(c => c.Marca)
                .Select(g => new { Marca = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad).Take(5).ToListAsync();
            vm.LabelsMarcas = datosMarcas.Select(x => x.Marca ?? "SIN MARCA").ToList();
            vm.DataMarcas = datosMarcas.Select(x => x.Cantidad).ToList();

            // 5. NUEVO: Top 5 Comisarías (SOLO SI ES ADMIN)
            if (!idDependencia.HasValue)
            {
                var datosComisarias = await _context.Celulars
                    .Include(c => c.IdPolicialRegistroNavigation)
                    .ThenInclude(p => p.IdDependenciaNavigation)
                    .Where(c => c.IdPolicialRegistroNavigation.IdDependenciaNavigation != null)
                    .GroupBy(c => c.IdPolicialRegistroNavigation.IdDependenciaNavigation.Nombre)
                    .Select(g => new { Comisaria = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(x => x.Cantidad).Take(5).ToListAsync();

                vm.LabelsComisarias = datosComisarias.Select(x => x.Comisaria).ToList();
                vm.DataComisarias = datosComisarias.Select(x => x.Cantidad).ToList();
            }

            // 6. Últimos Registros
            vm.UltimosRegistros = await query.Include(c => c.IdFiscaliaNavigation)
                .OrderByDescending(c => c.FechaRegistro).Take(5).ToListAsync();

            return vm;
        }
    }
}