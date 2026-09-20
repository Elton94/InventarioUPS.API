using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(InventarioUPSDbContext context)
        {
            // =====================================================
            // TIPOS DE AGENCIA
            // =====================================================

            var tipoLocal = await ObtenerOCrearTipoAgencia(
                context,
                "Local");

            var tipoForanea = await ObtenerOCrearTipoAgencia(
                context,
                "Foránea");

            // =====================================================
            // ESTADOS DE UPS
            // =====================================================

            await ObtenerOCrearEstadoUPS(context, "Operativo");
            await ObtenerOCrearEstadoUPS(context, "En mantenimiento");
            await ObtenerOCrearEstadoUPS(context, "Fuera de servicio");
            await ObtenerOCrearEstadoUPS(context, "De baja");
            await ObtenerOCrearEstadoUPS(context, "En resguardo");

            // =====================================================
            // TIPOS DE MANTENIMIENTO
            // =====================================================

            await ObtenerOCrearTipoMantenimiento(context, "Preventivo");
            await ObtenerOCrearTipoMantenimiento(context, "Correctivo");
            await ObtenerOCrearTipoMantenimiento(context, "Emergencia");

            // =====================================================
            // ESTADOS DE MANTENIMIENTO
            // =====================================================

            await ObtenerOCrearEstadoMantenimiento(context, "Programado");
            await ObtenerOCrearEstadoMantenimiento(context, "En proceso");
            await ObtenerOCrearEstadoMantenimiento(context, "Completado");
            await ObtenerOCrearEstadoMantenimiento(context, "Cancelado");

            // =====================================================
            // ROLES, PERMISOS Y POLÍTICA DE PRESUPUESTO
            // =====================================================

            await SembrarRolesPermisosAsync(context);
            await AsegurarPoliticaPresupuestoAsync(context);

            // =====================================================
            // DEPARTAMENTOS Y CIUDADES
            // =====================================================

            var departamentos = new Dictionary<string, string[]>
            {
                ["Atlántida"] = new[]
                {
                    "La Ceiba",
                    "Tela",
                    "Jutiapa",
                    "La Masica",
                    "San Francisco",
                    "Arizona",
                    "El Porvenir",
                    "Esparta"
                },

                ["Choluteca"] = new[]
                {
                    "Choluteca",
                    "Apacilagua",
                    "Concepción de María",
                    "Duyure",
                    "El Corpus",
                    "El Triunfo",
                    "Marcovia",
                    "Morolica",
                    "Namasigüe",
                    "Orocuina",
                    "Pespire",
                    "San Antonio de Flores",
                    "San Isidro",
                    "San José",
                    "Santa Ana de Yusguare"
                },

                ["Colón"] = new[]
                {
                    "Trujillo",
                    "Balfate",
                    "Bonito Oriental",
                    "Iriona",
                    "Limón",
                    "Sabá",
                    "Santa Fe",
                    "Santa Rosa de Aguán",
                    "Sonaguera"
                },

                ["Comayagua"] = new[]
                {
                    "Comayagua",
                    "Ajuterique",
                    "El Rosario",
                    "Esquías",
                    "Humuya",
                    "La Libertad",
                    "La Trinidad",
                    "Lejamaní",
                    "Lamaní",
                    "Meámbar",
                    "Minas de Oro",
                    "Ojos de Agua",
                    "San Jerónimo",
                    "San José de Comayagua",
                    "San Luis",
                    "San Sebastián",
                    "Siguatepeque",
                    "Villa de San Antonio"
                },

                ["Copán"] = new[]
                {
                    "Santa Rosa de Copán",
                    "Cabañas",
                    "Concepción",
                    "Copán Ruinas",
                    "Corquín",
                    "Cucuyagua",
                    "Dolores",
                    "Dulce Nombre",
                    "El Paraíso",
                    "Florida",
                    "La Jigua",
                    "La Unión",
                    "Nueva Arcadia",
                    "San Agustín",
                    "San Antonio",
                    "San Jerónimo",
                    "San José",
                    "San Juan de Opoa",
                    "San Nicolás",
                    "San Pedro",
                    "Santa Rita",
                    "Trinidad de Copán",
                    "Veracruz"
                },

                ["Cortés"] = new[]
                {
                    "San Pedro Sula",
                    "Choloma",
                    "La Lima",
                    "Puerto Cortés",
                    "Villanueva",
                    "Omoa",
                    "Pimienta",
                    "Potrerillos",
                    "San Antonio de Cortés",
                    "San Francisco de Yojoa",
                    "San Manuel",
                    "Santa Cruz de Yojoa"
                },

                ["El Paraíso"] = new[]
                {
                    "Yuscarán",
                    "Alauca",
                    "Danlí",
                    "El Paraíso",
                    "Güinope",
                    "Jacaleapa",
                    "Liure",
                    "Morocelí",
                    "Oropolí",
                    "Potrerillos",
                    "San Antonio de Flores",
                    "San Lucas",
                    "San Matías",
                    "Soledad",
                    "Teupasenti",
                    "Texiguat",
                    "Trojes",
                    "Vado Ancho"
                },

                ["Francisco Morazán"] = new[]
                {
                    "Distrito Central",
                    "Alubarén",
                    "Cedros",
                    "Curarén",
                    "El Porvenir",
                    "Guaimaca",
                    "La Libertad",
                    "La Venta",
                    "Lepaterique",
                    "Maraita",
                    "Marale",
                    "Nueva Armenia",
                    "Ojojona",
                    "Orica",
                    "Reitoca",
                    "Sabanagrande",
                    "San Antonio de Oriente",
                    "San Buenaventura",
                    "San Ignacio",
                    "San Juan de Flores",
                    "San Miguelito",
                    "Santa Ana",
                    "Santa Lucía",
                    "Talanga",
                    "Tatumbla",
                    "Valle de Ángeles",
                    "Villa de San Antonio"
                },

                ["Gracias a Dios"] = new[]
                {
                    "Puerto Lempira",
                    "Brus Laguna",
                    "Ahuas",
                    "Juan Francisco Bulnes",
                    "Ramón Villeda Morales",
                    "Wampusirpi"
                },

                ["Intibucá"] = new[]
                {
                    "La Esperanza",
                    "Camasca",
                    "Colomoncagua",
                    "Concepción",
                    "Dolores",
                    "Intibucá",
                    "Jesús de Otoro",
                    "Magdalena",
                    "Masaguara",
                    "San Antonio",
                    "San Francisco de Opalaca",
                    "San Isidro",
                    "San Juan"
                },

                ["Islas de la Bahía"] = new[]
                {
                    "Roatán",
                    "Guanaja",
                    "José Santos Guardiola",
                    "Utila"
                },

                ["La Paz"] = new[]
                {
                    "La Paz",
                    "Aguanqueterique",
                    "Cabañas",
                    "Cane",
                    "Chinacla",
                    "Guajiquiro",
                    "Lauterique",
                    "Marcala",
                    "Mercedes de Oriente",
                    "Opatoro",
                    "San Antonio del Norte",
                    "San José",
                    "San Juan",
                    "San Pedro de Tutule",
                    "Santa Ana",
                    "Santa Elena",
                    "Santa María",
                    "Santiago Puringla",
                    "Yarula"
                },

                ["Lempira"] = new[]
                {
                    "Gracias",
                    "Belén",
                    "Candelaria",
                    "Cololaca",
                    "Erandique",
                    "Gualcince",
                    "Guarita",
                    "La Campa",
                    "La Iguala",
                    "Las Flores",
                    "La Unión",
                    "La Virtud",
                    "Mapulaca",
                    "Piraera",
                    "San Andrés",
                    "San Francisco",
                    "San Manuel Colohete",
                    "San Marcos de Caiquín",
                    "San Rafael",
                    "San Sebastián",
                    "Santa Cruz",
                    "Talgua",
                    "Tambla",
                    "Tomalá",
                    "Valladolid",
                    "Virginia"
                },

                ["Ocotepeque"] = new[]
                {
                    "Nueva Ocotepeque",
                    "Belén Gualcho",
                    "Concepción",
                    "Dolores Merendón",
                    "Fraternidad",
                    "La Encarnación",
                    "La Labor",
                    "Lucerna",
                    "Mercedes",
                    "San Fernando",
                    "San Francisco del Valle",
                    "San Jorge",
                    "San Marcos",
                    "Santa Fe",
                    "Sensenti"
                },

                ["Olancho"] = new[]
                {
                    "Juticalpa",
                    "Campamento",
                    "Catacamas",
                    "Concordia",
                    "Dulce Nombre de Culmí",
                    "El Rosario",
                    "Esquipulas del Norte",
                    "Gualaco",
                    "Guarizama",
                    "Guata",
                    "Guayape",
                    "Jano",
                    "La Unión",
                    "Mangulile",
                    "Manto",
                    "Salamá",
                    "San Esteban",
                    "San Francisco de Becerra",
                    "San Francisco de la Paz",
                    "Santa María del Real",
                    "Silca",
                    "Yocón"
                },

                ["Santa Bárbara"] = new[]
                {
                    "Santa Bárbara",
                    "Arada",
                    "Atima",
                    "Azacualpa",
                    "Ceguaca",
                    "Chinda",
                    "Concepción del Norte",
                    "Concepción del Sur",
                    "El Níspero",
                    "Gualala",
                    "Ilama",
                    "Las Vegas",
                    "Macuelizo",
                    "Naranjito",
                    "Nueva Frontera",
                    "Nuevo Celilac",
                    "Petoa",
                    "Protección",
                    "Quimistán",
                    "San Francisco de Yojoa",
                    "San José de Colinas",
                    "San Luis",
                    "San Marcos",
                    "San Nicolás",
                    "San Pedro Zacapa",
                    "San Vicente Centenario",
                    "Trinidad"
                },

                ["Valle"] = new[]
                {
                    "Nacaome",
                    "Alianza",
                    "Amapala",
                    "Aramecina",
                    "Caridad",
                    "Goascorán",
                    "Langue",
                    "San Francisco de Coray",
                    "San Lorenzo"
                },

                ["Yoro"] = new[]
                {
                    "Yoro",
                    "Arenal",
                    "El Negrito",
                    "El Progreso",
                    "Jocón",
                    "Morazán",
                    "Olanchito",
                    "Santa Rita",
                    "Sulaco",
                    "Victoria",
                    "Yorito"
                }
            };

            foreach (var departamentoData in departamentos)
            {
                var departamento = await context.Departamentos
                    .FirstOrDefaultAsync(d =>
                        d.Nombre.ToLower() == departamentoData.Key.ToLower());

                if (departamento == null)
                {
                    departamento = new Departamento
                    {
                        Nombre = departamentoData.Key
                    };

                    context.Departamentos.Add(departamento);
                    await context.SaveChangesAsync();
                }

                foreach (var ciudadNombre in departamentoData.Value)
                {
                    var existeCiudad = await context.Ciudades.AnyAsync(c =>
                        c.DepartamentoId == departamento.Id &&
                        c.Nombre.ToLower() == ciudadNombre.ToLower());

                    if (!existeCiudad)
                    {
                        context.Ciudades.Add(new Ciudad
                        {
                            Nombre = ciudadNombre,
                            DepartamentoId = departamento.Id
                        });
                    }
                }

                await context.SaveChangesAsync();
            }

            // =====================================================
            // ELIMINAR "SIN ESPECIFICAR" SI QUEDÓ COMO DATO INICIAL
            // =====================================================

            var departamentosSinEspecificar =
                await context.Departamentos
                    .Where(d => d.Nombre == "Sin especificar")
                    .ToListAsync();

            foreach (var departamento in departamentosSinEspecificar)
            {
                var tieneCiudades = await context.Ciudades
                    .AnyAsync(c => c.DepartamentoId == departamento.Id);

                if (!tieneCiudades)
                    context.Departamentos.Remove(departamento);
            }

            var tiposAgenciaSinEspecificar =
                await context.TiposAgencia
                    .Where(t => t.Nombre == "Sin especificar")
                    .ToListAsync();

            foreach (var tipo in tiposAgenciaSinEspecificar)
            {
                var utilizado = await context.Agencias
                    .AnyAsync(a => a.TipoAgenciaId == tipo.Id);

                if (!utilizado)
                    context.TiposAgencia.Remove(tipo);
            }

            await context.SaveChangesAsync();

            // Mantener consistente la regla Local / Foránea también
            // para agencias creadas antes de esta versión.
            await NormalizarTiposAgenciaAsync(
                context,
                tipoLocal.Id,
                tipoForanea.Id);
        }

        // =====================================================
        // MÉTODOS AUXILIARES
        // =====================================================

        private static async Task NormalizarTiposAgenciaAsync(
            InventarioUPSDbContext context,
            int tipoLocalId,
            int tipoForaneaId)
        {
            var agencias = await context.Agencias
                .Include(a => a.Ciudad)
                .ToListAsync();

            foreach (var agencia in agencias)
            {
                var ciudad =
                    agencia.Ciudad?.Nombre?.Trim()
                    ?? string.Empty;

                var esLocal =
                    ciudad.Equals(
                        "San Pedro Sula",
                        StringComparison.OrdinalIgnoreCase) ||
                    ciudad.Equals(
                        "La Ceiba",
                        StringComparison.OrdinalIgnoreCase) ||
                    ciudad.Equals(
                        "Distrito Central",
                        StringComparison.OrdinalIgnoreCase);

                var tipoCorrecto =
                    esLocal
                        ? tipoLocalId
                        : tipoForaneaId;

                if (agencia.TipoAgenciaId != tipoCorrecto)
                {
                    agencia.TipoAgenciaId = tipoCorrecto;
                    agencia.FechaModificacion = DateTime.Now;
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task<TipoAgencia> ObtenerOCrearTipoAgencia(
            InventarioUPSDbContext context,
            string nombre)
        {
            var existente = await context.TiposAgencia
                .FirstOrDefaultAsync(x => x.Nombre == nombre);

            if (existente != null)
                return existente;

            var nuevo = new TipoAgencia
            {
                Nombre = nombre
            };

            context.TiposAgencia.Add(nuevo);
            await context.SaveChangesAsync();

            return nuevo;
        }

        private static async Task<EstadoUPS> ObtenerOCrearEstadoUPS(
            InventarioUPSDbContext context,
            string nombre)
        {
            var existente = await context.EstadosUPS
                .FirstOrDefaultAsync(x => x.Nombre == nombre);

            if (existente != null)
                return existente;

            var nuevo = new EstadoUPS
            {
                Nombre = nombre
            };

            context.EstadosUPS.Add(nuevo);
            await context.SaveChangesAsync();

            return nuevo;
        }

        private static async Task<TipoMantenimiento> ObtenerOCrearTipoMantenimiento(
            InventarioUPSDbContext context,
            string nombre)
        {
            var existente = await context.TiposMantenimiento
                .FirstOrDefaultAsync(x => x.Nombre == nombre);

            if (existente != null)
                return existente;

            var nuevo = new TipoMantenimiento
            {
                Nombre = nombre
            };

            context.TiposMantenimiento.Add(nuevo);
            await context.SaveChangesAsync();

            return nuevo;
        }

        private static async Task<EstadoMantenimiento> ObtenerOCrearEstadoMantenimiento(
            InventarioUPSDbContext context,
            string nombre)
        {
            var existente = await context.EstadosMantenimiento
                .FirstOrDefaultAsync(x => x.Nombre == nombre);

            if (existente != null)
                return existente;

            var nuevo = new EstadoMantenimiento
            {
                Nombre = nombre
            };

            context.EstadosMantenimiento.Add(nuevo);
            await context.SaveChangesAsync();

            return nuevo;
        }

        // =====================================================
        // ROLES Y PERMISOS BASE
        // =====================================================

        private static async Task SembrarRolesPermisosAsync(
            InventarioUPSDbContext context)
        {
            var definiciones = new[]
            {
                new { Codigo = "VER_INICIO", Nombre = "Ver inicio", Descripcion = "Acceso al resumen general." },
                new { Codigo = "VER_AGENCIAS", Nombre = "Ver agencias", Descripcion = "Consulta de agencias." },
                new { Codigo = "CREAR_AGENCIAS", Nombre = "Crear agencias", Descripcion = "Registro de agencias." },
                new { Codigo = "EDITAR_AGENCIAS", Nombre = "Editar agencias", Descripcion = "Edición y cambio de estado de agencias." },
                new { Codigo = "VER_UPS", Nombre = "Ver UPS", Descripcion = "Consulta del inventario UPS." },
                new { Codigo = "CREAR_UPS", Nombre = "Crear UPS", Descripcion = "Registro de UPS." },
                new { Codigo = "EDITAR_UPS", Nombre = "Editar UPS", Descripcion = "Edición y cambio de estado de UPS." },
                new { Codigo = "VER_MANTENIMIENTO", Nombre = "Ver mantenimiento", Descripcion = "Consulta de mantenimientos." },
                new { Codigo = "CREAR_MANTENIMIENTO", Nombre = "Crear mantenimiento", Descripcion = "Registro de mantenimientos." },
                new { Codigo = "EDITAR_MANTENIMIENTO", Nombre = "Editar mantenimiento", Descripcion = "Actualización de mantenimientos." },
                new { Codigo = "VER_PRESUPUESTO", Nombre = "Ver presupuesto", Descripcion = "Consulta de proyecciones de baterías." },
                new { Codigo = "GESTIONAR_PRESUPUESTO", Nombre = "Gestionar presupuesto", Descripcion = "Configuración de costos y política de reemplazo." },
                new { Codigo = "GESTIONAR_USUARIOS", Nombre = "Gestionar usuarios", Descripcion = "Administración de usuarios." },
                new { Codigo = "GESTIONAR_ROLES_PERMISOS", Nombre = "Gestionar roles y permisos", Descripcion = "Administración de roles y permisos." }
            };

            var permisos = new Dictionary<string, Permiso>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var item in definiciones)
            {
                var permiso = await context.Permisos
                    .FirstOrDefaultAsync(p => p.Codigo == item.Codigo);

                if (permiso == null)
                {
                    permiso = new Permiso
                    {
                        Codigo = item.Codigo,
                        Nombre = item.Nombre,
                        Descripcion = item.Descripcion,
                        Activo = true
                    };

                    context.Permisos.Add(permiso);
                }
                else
                {
                    permiso.Nombre = item.Nombre;
                    permiso.Descripcion = item.Descripcion;
                    permiso.Activo = true;
                }

                permisos[item.Codigo] = permiso;
            }

            await context.SaveChangesAsync();

            // =================================================
            // PERMISOS OBSOLETOS
            // Reportes fue eliminado del MVP. Si el permiso quedó
            // de una versión anterior, se desactiva para que no
            // aparezca en la administración de roles.
            // =================================================

            var permisosObsoletos = await context.Permisos
                .Where(p => p.Codigo == "VER_REPORTES")
                .ToListAsync();

            foreach (var permisoObsoleto in permisosObsoletos)
            {
                permisoObsoleto.Activo = false;
                permisoObsoleto.FechaModificacion = DateTime.Now;
            }

            await context.SaveChangesAsync();

            var controlTotal = await ObtenerOCrearRol(
                context, "Control Total", "Acceso completo al sistema");

            var administrador = await ObtenerOCrearRol(
                context, "Administrador", "Administración operativa del sistema");

            var tecnico = await ObtenerOCrearRol(
                context, "Técnico", "Gestión técnica de UPS y mantenimientos");

            var consulta = await ObtenerOCrearRol(
                context, "Consulta", "Acceso de solo consulta");

            await AsegurarPermisosRol(
                context,
                controlTotal,
                permisos.Values.Select(p => p.Id),
                forzarSincronizacion: true);

            await AsegurarPermisosRol(
                context,
                administrador,
                new[]
                {
                    permisos["VER_INICIO"].Id,
                    permisos["VER_AGENCIAS"].Id,
                    permisos["CREAR_AGENCIAS"].Id,
                    permisos["EDITAR_AGENCIAS"].Id,
                    permisos["VER_UPS"].Id,
                    permisos["CREAR_UPS"].Id,
                    permisos["EDITAR_UPS"].Id,
                    permisos["VER_MANTENIMIENTO"].Id,
                    permisos["CREAR_MANTENIMIENTO"].Id,
                    permisos["EDITAR_MANTENIMIENTO"].Id,
                    permisos["VER_PRESUPUESTO"].Id,
                    permisos["GESTIONAR_PRESUPUESTO"].Id,
                    permisos["GESTIONAR_USUARIOS"].Id
                });

            await AsegurarPermisosRol(
                context,
                tecnico,
                new[]
                {
                    permisos["VER_INICIO"].Id,
                    permisos["VER_AGENCIAS"].Id,
                    permisos["VER_UPS"].Id,
                    permisos["CREAR_UPS"].Id,
                    permisos["EDITAR_UPS"].Id,
                    permisos["VER_MANTENIMIENTO"].Id,
                    permisos["CREAR_MANTENIMIENTO"].Id,
                    permisos["EDITAR_MANTENIMIENTO"].Id
                });

            await AsegurarPermisosRol(
                context,
                consulta,
                new[]
                {
                    permisos["VER_INICIO"].Id,
                    permisos["VER_AGENCIAS"].Id,
                    permisos["VER_UPS"].Id,
                    permisos["VER_MANTENIMIENTO"].Id,
                    permisos["VER_PRESUPUESTO"].Id
                });
        }

        private static async Task<Rol> ObtenerOCrearRol(
            InventarioUPSDbContext context,
            string nombre,
            string descripcion)
        {
            var rol = await context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == nombre);

            if (rol != null)
                return rol;

            rol = new Rol
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Activo = true
            };

            context.Roles.Add(rol);
            await context.SaveChangesAsync();

            return rol;
        }

        private static async Task AsegurarPermisosRol(
            InventarioUPSDbContext context,
            Rol rol,
            IEnumerable<int> idsPermiso,
            bool forzarSincronizacion = false)
        {
            var ids = idsPermiso.Distinct().ToList();

            var actuales = await context.RolPermisos
                .Where(rp => rp.RolId == rol.Id)
                .ToListAsync();

            if (!forzarSincronizacion && actuales.Count > 0)
                return;

            if (forzarSincronizacion)
                context.RolPermisos.RemoveRange(actuales);

            foreach (var permisoId in ids)
            {
                if (!forzarSincronizacion &&
                    actuales.Any(rp => rp.PermisoId == permisoId))
                    continue;

                context.RolPermisos.Add(new RolPermiso
                {
                    RolId = rol.Id,
                    PermisoId = permisoId,
                    FechaCreacion = DateTime.Now
                });
            }

            await context.SaveChangesAsync();
        }

        // =====================================================
        // POLÍTICA GLOBAL ÚNICA
        // =====================================================

        private static async Task AsegurarPoliticaPresupuestoAsync(
            InventarioUPSDbContext context)
        {
            var politicas = await context.PoliticaReemplazoBaterias
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            if (politicas.Count == 0)
            {
                context.PoliticaReemplazoBaterias.Add(
                    new PoliticaReemplazoBaterias
                    {
                        MesesReemplazoLocal = 24,
                        MesesReemplazoForanea = 18,
                        FechaCreacion = DateTime.Now
                    });

                await context.SaveChangesAsync();
                return;
            }

            if (politicas.Count > 1)
            {
                context.PoliticaReemplazoBaterias.RemoveRange(
                    politicas.Skip(1));

                await context.SaveChangesAsync();
            }
        }

    }
}
