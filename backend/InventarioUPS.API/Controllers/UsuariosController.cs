using InventarioUPS.API.Authorization;
using InventarioUPS.API.Data;
using InventarioUPS.API.DTOs;
using InventarioUPS.API.Models;
using InventarioUPS.API.Repositories;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InventarioUPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private const string RolControlTotal = "Control Total";
        private const string PermisoGestionarRoles =
            "GESTIONAR_ROLES_PERMISOS";

        private readonly IUsuarioRepository _repository;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly InventarioUPSDbContext _context;

        public UsuariosController(
            IUsuarioRepository repository,
            IPasswordHasher<Usuario> passwordHasher,
            InventarioUPSDbContext context)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _context = context;
        }

        // =====================================================
        // USUARIOS
        // =====================================================

        [HttpGet]
        [PermisoAuthorize("GESTIONAR_USUARIOS")]
        public async Task<ActionResult<List<UsuarioDTO>>>
            ObtenerTodos()
        {
            var usuarios =
                await _repository.ObtenerTodosAsync();

            var resultado =
                usuarios
                    .Select(u => new UsuarioDTO
                    {
                        Id = u.Id,
                        NombreUsuario = u.NombreUsuario,
                        NombreCompleto = u.NombreCompleto,
                        Correo = u.Correo,
                        RolId = u.RolId,
                        RolNombre =
                            u.Rol?.Nombre ?? string.Empty,
                        Activo = u.Activo
                    })
                    .ToList();

            return Ok(resultado);
        }

        // =====================================================
        // OBTENER USUARIO POR ID
        // =====================================================

        [HttpGet("{id:int}")]
        [PermisoAuthorize("GESTIONAR_USUARIOS")]
        public async Task<ActionResult<UsuarioDTO>>
            ObtenerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador del usuario no es válido.");
            }

            var usuario =
                await _repository.ObtenerPorIdAsync(id);

            if (usuario == null)
            {
                return NotFound(
                    "El usuario no existe.");
            }

            return Ok(new UsuarioDTO
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                RolId = usuario.RolId,
                RolNombre =
                    usuario.Rol?.Nombre ?? string.Empty,
                Activo = usuario.Activo
            });
        }

        // =====================================================
        // ROLES
        // =====================================================

        [HttpGet("roles")]
        [PermisoAuthorize("GESTIONAR_USUARIOS", "GESTIONAR_ROLES_PERMISOS")]
        public async Task<ActionResult<List<Rol>>>
            ObtenerRoles()
        {
            var roles =
                await _repository.ObtenerRolesAsync();

            return Ok(roles);
        }

        // =====================================================
        // RESPONSABLES DE MANTENIMIENTO
        // Usuarios activos cuyo rol puede crear o editar mantenimiento.
        // Control Total también se incluye.
        // =====================================================

        [HttpGet("responsables-mantenimiento")]
        [PermisoAuthorize("CREAR_MANTENIMIENTO", "EDITAR_MANTENIMIENTO")]
        public async Task<ActionResult> ObtenerResponsablesMantenimiento()
        {
            var rolesConPermiso = await _context.RolPermisos
                .AsNoTracking()
                .Where(rp =>
                    rp.Permiso != null &&
                    rp.Permiso.Activo &&
                    (rp.Permiso.Codigo == "CREAR_MANTENIMIENTO" ||
                     rp.Permiso.Codigo == "EDITAR_MANTENIMIENTO"))
                .Select(rp => rp.RolId)
                .Distinct()
                .ToListAsync();

            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .Where(u =>
                    u.Activo &&
                    u.Rol != null &&
                    u.Rol.Activo &&
                    (u.Rol.Nombre == RolControlTotal ||
                     rolesConPermiso.Contains(u.RolId)))
                .OrderBy(u => u.NombreCompleto)
                .Select(u => new
                {
                    id = u.Id,
                    nombreUsuario = u.NombreUsuario,
                    nombreCompleto = u.NombreCompleto,
                    rolNombre = u.Rol!.Nombre
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // =====================================================
        // OBTENER ROL POR ID
        // =====================================================

        [HttpGet("roles/{id:int}")]
        [PermisoAuthorize("GESTIONAR_ROLES_PERMISOS")]
        public async Task<ActionResult<Rol>>
            ObtenerRol(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador del rol no es válido.");
            }

            var rol =
                await _repository.ObtenerRolPorIdAsync(id);

            if (rol == null)
            {
                return NotFound(
                    "El rol no existe.");
            }

            return Ok(rol);
        }

        // =====================================================
        // CREAR USUARIO
        // =====================================================

        [HttpPost]
        [PermisoAuthorize("GESTIONAR_USUARIOS")]
        public async Task<ActionResult>
            Crear(CrearUsuarioDTO request)
        {
            if (request == null)
            {
                return BadRequest(
                    "La información del usuario es obligatoria.");
            }

            var nombreUsuario =
                request.NombreUsuario?.Trim()
                ?? string.Empty;

            var nombreCompleto =
                request.NombreCompleto?.Trim()
                ?? string.Empty;

            var correo =
                request.Correo?.Trim()
                ?? string.Empty;

            var password =
                request.Password
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return BadRequest(
                    "El nombre de usuario es obligatorio.");
            }

            if (nombreUsuario.Length < 3)
            {
                return BadRequest(
                    "El nombre de usuario debe tener al menos 3 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                return BadRequest(
                    "El nombre completo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                return BadRequest(
                    "El correo electrónico es obligatorio.");
            }

            if (!EsCorreoValido(correo))
            {
                return BadRequest(
                    "El correo electrónico no tiene un formato válido.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(
                    "La contraseña es obligatoria.");
            }

            if (!CumplePoliticaPassword(password))
            {
                return BadRequest(
                    "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
            }

            if (request.RolId <= 0)
            {
                return BadRequest(
                    "Debe seleccionar un rol.");
            }

            if (await _repository
                .ExisteNombreUsuarioAsync(nombreUsuario))
            {
                return Conflict(
                    "El nombre de usuario ya existe.");
            }

            if (await _repository
                .ExisteCorreoAsync(correo))
            {
                return Conflict(
                    "El correo electrónico ya está registrado.");
            }

            var rol =
                await _context.Roles
                    .FirstOrDefaultAsync(r =>
                        r.Id == request.RolId &&
                        r.Activo);

            if (rol == null)
            {
                return BadRequest(
                    "El rol seleccionado no existe o está inactivo.");
            }

            // -------------------------------------------------
            // PROTEGER ASIGNACIÓN DE CONTROL TOTAL
            // -------------------------------------------------

            if (EsControlTotal(rol.Nombre) &&
                !await TienePermisoGestionarRolesAsync())
            {
                return Forbid();
            }

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                NombreCompleto = nombreCompleto,
                Correo = correo,
                RolId = rol.Id,
                Activo = true
            };

            usuario.PasswordHash =
                _passwordHasher.HashPassword(
                    usuario,
                    password);

            await _repository.CrearAsync(usuario);

            return Ok(new
            {
                mensaje =
                    "Usuario creado correctamente.",

                usuarioId =
                    usuario.Id
            });
        }

        // =====================================================
        // ACTUALIZAR USUARIO
        // =====================================================

        [HttpPut("{id:int}")]
        [PermisoAuthorize("GESTIONAR_USUARIOS")]
        public async Task<ActionResult>
            Actualizar(
                int id,
                ActualizarUsuarioDTO request)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador del usuario no es válido.");
            }

            if (request == null)
            {
                return BadRequest(
                    "La información del usuario es obligatoria.");
            }

            var usuario =
                await _repository.ObtenerPorIdAsync(id);

            if (usuario == null)
            {
                return NotFound(
                    "El usuario no existe.");
            }

            var usuarioActualId = ObtenerUsuarioActualId();

            if (!request.Activo &&
                usuarioActualId.HasValue &&
                usuarioActualId.Value == id)
            {
                return BadRequest(
                    "No puede desactivar su propio usuario desde la sesión actual.");
            }

            var nombreCompleto =
                request.NombreCompleto?.Trim()
                ?? string.Empty;

            var correo =
                request.Correo?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                return BadRequest(
                    "El nombre completo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                return BadRequest(
                    "El correo electrónico es obligatorio.");
            }

            if (!EsCorreoValido(correo))
            {
                return BadRequest(
                    "El correo electrónico no tiene un formato válido.");
            }

            if (request.RolId <= 0)
            {
                return BadRequest(
                    "Debe seleccionar un rol.");
            }

            var rol =
                await _context.Roles
                    .FirstOrDefaultAsync(r =>
                        r.Id == request.RolId &&
                        r.Activo);

            if (rol == null)
            {
                return BadRequest(
                    "El rol seleccionado no existe o está inactivo.");
            }

            var correoExiste =
                await _context.Usuarios
                    .AnyAsync(u =>
                        u.Id != id &&
                        u.Correo.ToUpper()
                        == correo.ToUpper());

            if (correoExiste)
            {
                return Conflict(
                    "El correo electrónico ya está registrado.");
            }

            var rolActual =
                usuario.Rol?.Nombre
                ?? string.Empty;

            var esActualmenteControlTotal =
                EsControlTotal(rolActual);

            var nuevoEsControlTotal =
                EsControlTotal(rol.Nombre);

            // -------------------------------------------------
            // PROTEGER ASIGNACIÓN DE CONTROL TOTAL
            // -------------------------------------------------

            if (nuevoEsControlTotal &&
                !esActualmenteControlTotal &&
                !await TienePermisoGestionarRolesAsync())
            {
                return Forbid();
            }

            // -------------------------------------------------
            // PROTEGER ÚLTIMO CONTROL TOTAL
            // -------------------------------------------------

            if (esActualmenteControlTotal &&
                usuario.Activo &&
                (!request.Activo ||
                 !nuevoEsControlTotal))
            {
                var cantidadControlTotalActivos =
                    await _repository
                        .ContarUsuariosActivosPorRolAsync(
                            usuario.RolId);

                if (cantidadControlTotalActivos <= 1)
                {
                    return BadRequest(
                        "No se puede desactivar ni cambiar de rol al último usuario activo con Control Total.");
                }
            }

            usuario.NombreCompleto =
                nombreCompleto;

            usuario.Correo =
                correo;

            usuario.RolId =
                request.RolId;

            usuario.Activo =
                request.Activo;

            if (!string.IsNullOrWhiteSpace(request.NuevaPassword))
            {
                if (!CumplePoliticaPassword(request.NuevaPassword))
                {
                    return BadRequest(
                        "La nueva contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
                }

                usuario.PasswordHash =
                    _passwordHasher.HashPassword(
                        usuario,
                        request.NuevaPassword);
            }

            usuario.FechaModificacion =
                DateTime.Now;

            await _repository.ActualizarAsync(usuario);

            return Ok(
                "Usuario actualizado correctamente.");
        }

        // =====================================================
        // ACTIVAR / DESACTIVAR USUARIO
        // =====================================================

        [HttpPatch("{id:int}/estado")]
        [PermisoAuthorize("GESTIONAR_USUARIOS")]
        public async Task<ActionResult>
            CambiarEstado(
                int id,
                [FromQuery] bool activo)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador del usuario no es válido.");
            }

            var usuario =
                await _repository.ObtenerPorIdAsync(id);

            if (usuario == null)
            {
                return NotFound(
                    "El usuario no existe.");
            }

            var usuarioActualId = ObtenerUsuarioActualId();

            if (!activo &&
                usuarioActualId.HasValue &&
                usuarioActualId.Value == id)
            {
                return BadRequest(
                    "No puede desactivar su propio usuario desde la sesión actual.");
            }

            var rolNombre =
                usuario.Rol?.Nombre
                ?? string.Empty;

            var esControlTotal =
                EsControlTotal(rolNombre);

            if (!activo &&
                usuario.Activo &&
                esControlTotal)
            {
                var cantidadControlTotalActivos =
                    await _repository
                        .ContarUsuariosActivosPorRolAsync(
                            usuario.RolId);

                if (cantidadControlTotalActivos <= 1)
                {
                    return BadRequest(
                        "No se puede desactivar al último usuario activo con Control Total.");
                }
            }

            usuario.Activo =
                activo;

            usuario.FechaModificacion =
                DateTime.Now;

            await _repository.ActualizarAsync(usuario);

            return Ok(new
            {
                mensaje = activo
                    ? "Usuario activado correctamente."
                    : "Usuario desactivado correctamente."
            });
        }

        // =====================================================
        // CREAR ROL
        // =====================================================

        [HttpPost("roles")]
        [PermisoAuthorize("GESTIONAR_ROLES_PERMISOS")]
        public async Task<ActionResult>
            CrearRol(Rol request)
        {
            if (request == null)
            {
                return BadRequest(
                    "La información del rol es obligatoria.");
            }

            var nombre =
                request.Nombre?.Trim()
                ?? string.Empty;

            var descripcion =
                request.Descripcion?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(
                    "El nombre del rol es obligatorio.");
            }

            if (nombre.Length < 3)
            {
                return BadRequest(
                    "El nombre del rol debe tener al menos 3 caracteres.");
            }

            var existe =
                await _context.Roles
                    .AnyAsync(r =>
                        r.Nombre.ToUpper()
                        == nombre.ToUpper());

            if (existe)
            {
                return Conflict(
                    "Ya existe un rol con ese nombre.");
            }

            var rol = new Rol
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Activo = true
            };

            await _repository.CrearRolAsync(rol);

            return Ok(new
            {
                mensaje =
                    "Rol creado correctamente.",

                rolId =
                    rol.Id
            });
        }

        // =====================================================
        // ACTUALIZAR ROL
        // =====================================================

        [HttpPut("roles/{id:int}")]
        [PermisoAuthorize("GESTIONAR_ROLES_PERMISOS")]
        public async Task<ActionResult>
            ActualizarRol(
                int id,
                Rol request)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador del rol no es válido.");
            }

            if (request == null)
            {
                return BadRequest(
                    "La información del rol es obligatoria.");
            }

            var rol =
                await _repository.ObtenerRolPorIdAsync(id);

            if (rol == null)
            {
                return NotFound(
                    "El rol no existe.");
            }

            var nombre =
                request.Nombre?.Trim()
                ?? string.Empty;

            var descripcion =
                request.Descripcion?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(
                    "El nombre del rol es obligatorio.");
            }

            if (nombre.Length < 3)
            {
                return BadRequest(
                    "El nombre del rol debe tener al menos 3 caracteres.");
            }

            var existe =
                await _context.Roles
                    .AnyAsync(r =>
                        r.Id != id &&
                        r.Nombre.ToUpper()
                        == nombre.ToUpper());

            if (existe)
            {
                return Conflict(
                    "Ya existe otro rol con ese nombre.");
            }

            var esControlTotal =
                EsControlTotal(rol.Nombre);

            if (esControlTotal &&
                !EsControlTotal(nombre))
            {
                return BadRequest(
                    "El rol Control Total no puede ser renombrado.");
            }

            // -------------------------------------------------
            // NO DESACTIVAR NINGÚN ROL CON USUARIOS ACTIVOS
            // -------------------------------------------------

            if (!request.Activo &&
                rol.Activo)
            {
                var usuariosActivos =
                    await _repository
                        .ContarUsuariosActivosPorRolAsync(
                            rol.Id);

                if (usuariosActivos > 0)
                {
                    if (esControlTotal)
                    {
                        return BadRequest(
                            "No se puede desactivar el rol Control Total porque tiene usuarios activos.");
                    }

                    return BadRequest(
                        "No se puede desactivar el rol porque tiene usuarios activos asignados.");
                }
            }

            rol.Nombre =
                esControlTotal
                    ? RolControlTotal
                    : nombre;

            rol.Descripcion =
                descripcion;

            rol.Activo =
                request.Activo;

            rol.FechaModificacion =
                DateTime.Now;

            await _repository.ActualizarRolAsync(rol);

            return Ok(
                "Rol actualizado correctamente.");
        }

        // =====================================================
        // ACTIVAR / DESACTIVAR ROL
        // =====================================================

        [HttpPatch("roles/{id:int}/estado")]
        [PermisoAuthorize("GESTIONAR_ROLES_PERMISOS")]
        public async Task<ActionResult>
            CambiarEstadoRol(
                int id,
                [FromQuery] bool activo)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador del rol no es válido.");
            }

            var rol =
                await _repository
                    .ObtenerRolPorIdAsync(id);

            if (rol == null)
            {
                return NotFound(
                    "El rol no existe.");
            }

            // -------------------------------------------------
            // NO DESACTIVAR NINGÚN ROL CON USUARIOS ACTIVOS
            // -------------------------------------------------

            if (!activo && rol.Activo)
            {
                var usuariosActivos =
                    await _repository
                        .ContarUsuariosActivosPorRolAsync(
                            rol.Id);

                if (usuariosActivos > 0)
                {
                    if (EsControlTotal(rol.Nombre))
                    {
                        return BadRequest(
                            "No se puede desactivar el rol Control Total porque tiene usuarios activos.");
                    }

                    return BadRequest(
                        "No se puede desactivar el rol porque tiene usuarios activos asignados.");
                }
            }

            rol.Activo =
                activo;

            rol.FechaModificacion =
                DateTime.Now;

            await _repository.ActualizarRolAsync(rol);

            return Ok(new
            {
                mensaje = activo
                    ? "Rol activado correctamente."
                    : "Rol desactivado correctamente."
            });
        }

        // =====================================================
        // PERMISOS
        // =====================================================

        [HttpGet("permisos")]
        [PermisoAuthorize("GESTIONAR_ROLES_PERMISOS")]
        public async Task<ActionResult>
            ObtenerPermisos()
        {
            var permisos =
                await _context.Permisos
                    .AsNoTracking()
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .Select(p => new
                    {
                        id = p.Id,
                        nombre = p.Nombre,
                        codigo = p.Codigo,
                        descripcion = p.Descripcion
                    })
                    .ToListAsync();

            return Ok(permisos);
        }

        // =====================================================
        // OBTENER PERMISOS DE UN ROL
        // =====================================================

        [HttpGet("roles/{id:int}/permisos")]
        [PermisoAuthorize("GESTIONAR_ROLES_PERMISOS")]
        public async Task<ActionResult>
            ObtenerPermisosRol(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador del rol no es válido.");
            }

            var rolExiste =
                await _context.Roles
                    .AnyAsync(r => r.Id == id);

            if (!rolExiste)
            {
                return NotFound(
                    "El rol no existe.");
            }

            var permisos =
                await _context.RolPermisos
                    .AsNoTracking()
                    .Where(rp =>
                        rp.RolId == id &&
                        rp.Permiso != null &&
                        rp.Permiso.Activo)
                    .Select(rp => new
                    {
                        id = rp.PermisoId,
                        nombre = rp.Permiso!.Nombre,
                        codigo = rp.Permiso.Codigo
                    })
                    .OrderBy(p => p.nombre)
                    .ToListAsync();

            return Ok(permisos);
        }

        // =====================================================
        // ACTUALIZAR PERMISOS DE UN ROL
        // =====================================================

        [HttpPut("roles/{id:int}/permisos")]
        [PermisoAuthorize("GESTIONAR_ROLES_PERMISOS")]
        public async Task<ActionResult>
            ActualizarPermisosRol(
                int id,
                ActualizarPermisosRolRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador del rol no es válido.");
            }

            if (request == null)
            {
                return BadRequest(
                    "La información de permisos es obligatoria.");
            }

            var rol =
                await _context.Roles
                    .FirstOrDefaultAsync(
                        r => r.Id == id);

            if (rol == null)
            {
                return NotFound(
                    "El rol no existe.");
            }

            if (!rol.Activo)
            {
                return BadRequest(
                    "No se pueden modificar los permisos de un rol inactivo.");
            }

            var esControlTotal =
                EsControlTotal(rol.Nombre);

            var permisosActivos =
                await _context.Permisos
                    .Where(p => p.Activo)
                    .ToListAsync();

            List<int> permisoIds;

            if (esControlTotal)
            {
                permisoIds =
                    permisosActivos
                        .Select(p => p.Id)
                        .ToList();
            }
            else
            {
                permisoIds =
                    (request.PermisoIds ??
                     new List<int>())
                    .Distinct()
                    .ToList();

                var idsValidos =
                    permisosActivos
                        .Select(p => p.Id)
                        .ToHashSet();

                var idsInvalidos =
                    permisoIds
                        .Where(idPermiso =>
                            !idsValidos.Contains(idPermiso))
                        .ToList();

                if (idsInvalidos.Any())
                {
                    return BadRequest(
                        "Uno o más permisos seleccionados no existen o están inactivos.");
                }
            }

            var permisosActuales =
                await _context.RolPermisos
                    .Where(rp =>
                        rp.RolId == id)
                    .ToListAsync();

            _context.RolPermisos.RemoveRange(
                permisosActuales);

            foreach (var permisoId in permisoIds)
            {
                _context.RolPermisos.Add(
                    new RolPermiso
                    {
                        RolId = id,
                        PermisoId = permisoId,
                        FechaCreacion = DateTime.Now
                    });
            }

            rol.FechaModificacion =
                DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "Permisos del rol actualizados correctamente.",

                rolId =
                    id,

                cantidadPermisos =
                    permisoIds.Count
            });
        }

        // =====================================================
        // MÉTODOS AUXILIARES
        // =====================================================

        private static bool EsControlTotal(
            string? nombreRol)
        {
            return string.Equals(
                nombreRol?.Trim(),
                RolControlTotal,
                StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> TienePermisoGestionarRolesAsync()
        {
            var usuarioId = ObtenerUsuarioActualId();

            if (!usuarioId.HasValue)
                return false;

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.Id == usuarioId.Value &&
                    u.Activo);

            if (usuario?.Rol == null || !usuario.Rol.Activo)
                return false;

            if (EsControlTotal(usuario.Rol.Nombre))
                return true;

            return await _context.RolPermisos
                .AsNoTracking()
                .AnyAsync(rp =>
                    rp.RolId == usuario.RolId &&
                    rp.Permiso != null &&
                    rp.Permiso.Activo &&
                    rp.Permiso.Codigo == PermisoGestionarRoles);
        }

        private int? ObtenerUsuarioActualId()
        {
            var valor =
                HttpContext.User
                    .FindFirst(ClaimTypes.NameIdentifier)
                    ?.Value;

            return int.TryParse(valor, out var id)
                ? id
                : null;
        }

        private static bool EsCorreoValido(
            string correo)
        {
            try
            {
                var direccion =
                    new System.Net.Mail.MailAddress(
                        correo);

                return string.Equals(
                    direccion.Address,
                    correo,
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool CumplePoliticaPassword(
            string password)
        {
            if (password.Length < 8)
            {
                return false;
            }

            var tieneMayuscula =
                password.Any(char.IsUpper);

            var tieneMinuscula =
                password.Any(char.IsLower);

            var tieneNumero =
                password.Any(char.IsDigit);

            var tieneEspecial =
                password.Any(
                    c => !char.IsLetterOrDigit(c));

            return tieneMayuscula &&
                   tieneMinuscula &&
                   tieneNumero &&
                   tieneEspecial;
        }
    }

    // =========================================================
    // REQUEST PARA ACTUALIZAR PERMISOS
    // =========================================================

    public class ActualizarPermisosRolRequest
    {
        public List<int> PermisoIds { get; set; }
            = new List<int>();
    }
}