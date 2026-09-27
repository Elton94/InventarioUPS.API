using InventarioUPS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioUPS.API.Data
{
    public class InventarioUPSDbContext : DbContext
    {
        public InventarioUPSDbContext(
            DbContextOptions<InventarioUPSDbContext> options)
            : base(options)
        {
        }

        public DbSet<MarcaUPS> MarcasUPS => Set<MarcaUPS>();
        public DbSet<ModeloUPS> ModelosUPS => Set<ModeloUPS>();
        public DbSet<AuditoriaCatalogo> AuditoriaCatalogos => Set<AuditoriaCatalogo>();
        public DbSet<TipoBateria> TiposBateria { get; set; }
        public DbSet<ModeloBateria> ModelosBateria { get; set; }
        public DbSet<CostoModeloBateria> CostosModeloBateria { get; set; }

        // TABLAS
        public DbSet<Permiso> Permisos { get; set; }

        public DbSet<RolPermiso> RolPermisos { get; set; }

        public DbSet<Agencia> Agencias { get; set; }

        public DbSet<Departamento> Departamentos { get; set; }

        public DbSet<Ciudad> Ciudades { get; set; }

        public DbSet<TipoAgencia> TiposAgencia { get; set; }

        public DbSet<UPS> UPSs { get; set; }

        public DbSet<EstadoUPS> EstadosUPS { get; set; }

        public DbSet<Mantenimiento> Mantenimientos { get; set; }

        public DbSet<EstadoMantenimiento> EstadosMantenimiento { get; set; }

        public DbSet<Rol> Roles { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<ConfiguracionPresupuestoBaterias>
            ConfiguracionPresupuestoBaterias
        { get; set; }

        public DbSet<PoliticaReemplazoBaterias>
            PoliticaReemplazoBaterias
        { get; set; }

        // CONFIGURACIÓN
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoBateria>().Property(x => x.Nombre).HasMaxLength(80).IsRequired();
            modelBuilder.Entity<TipoBateria>().HasIndex(x => x.Nombre).IsUnique();
            modelBuilder.Entity<ModeloBateria>().Property(x => x.Nombre).HasMaxLength(150).UseCollation("Latin1_General_100_CI_AS").IsRequired();
            modelBuilder.Entity<ModeloBateria>().HasIndex(x => new { x.TipoBateriaId, x.Nombre }).IsUnique();
            modelBuilder.Entity<ModeloBateria>().HasOne(x => x.TipoBateria).WithMany().HasForeignKey(x => x.TipoBateriaId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UPS>().HasOne(x => x.ModeloBateria).WithMany().HasForeignKey(x => x.ModeloBateriaId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UPS>().HasOne<ModeloBateria>().WithMany().HasForeignKey(x => x.ModeloBateriaBaseId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Mantenimiento>().HasOne(x => x.ModeloBateria).WithMany().HasForeignKey(x => x.ModeloBateriaId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CostoModeloBateria>().Property(x => x.CostoUnitario).HasPrecision(18, 2);
            modelBuilder.Entity<CostoModeloBateria>().HasIndex(x => new { x.Anio, x.ModeloBateriaId }).IsUnique();
            modelBuilder.Entity<CostoModeloBateria>().HasOne(x => x.ModeloBateria).WithMany().HasForeignKey(x => x.ModeloBateriaId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarcaUPS>().Property(x => x.Nombre).HasMaxLength(100).UseCollation("Latin1_General_100_CI_AS").IsRequired();
            modelBuilder.Entity<MarcaUPS>().HasIndex(x => x.Nombre).IsUnique();
            modelBuilder.Entity<ModeloUPS>().Property(x => x.Nombre).HasMaxLength(100).UseCollation("Latin1_General_100_CI_AS").IsRequired();
            modelBuilder.Entity<ModeloUPS>().HasIndex(x => new { x.MarcaUPSId, x.Nombre }).IsUnique();
            modelBuilder.Entity<ModeloUPS>().HasOne(x => x.MarcaUPS).WithMany().HasForeignKey(x => x.MarcaUPSId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UPS>().HasOne(x => x.ModeloUPS).WithMany().HasForeignKey(x => x.ModeloUPSId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AuditoriaCatalogo>().Property(x => x.Usuario).HasMaxLength(200);
            modelBuilder.Entity<AuditoriaCatalogo>().Property(x => x.Catalogo).HasMaxLength(50);
            modelBuilder.Entity<AuditoriaCatalogo>().Property(x => x.Accion).HasMaxLength(30);
            modelBuilder.Entity<AuditoriaCatalogo>().HasIndex(x => x.Fecha);
            modelBuilder.Entity<Departamento>().Property(x => x.Nombre).UseCollation("Latin1_General_100_CI_AS");
            modelBuilder.Entity<Ciudad>().Property(x => x.Nombre).UseCollation("Latin1_General_100_CI_AS");
            modelBuilder.Entity<TipoBateria>().Property(x => x.Nombre).UseCollation("Latin1_General_100_CI_AS");

            // DEPARTAMENTOS
            modelBuilder.Entity<Departamento>()
                .Property(d => d.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Departamento>()
                .HasIndex(d => d.Nombre)
                .IsUnique();

            // CIUDADES
            modelBuilder.Entity<Ciudad>()
                .Property(c => c.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Ciudad>()
                .HasOne(c => c.Departamento)
                .WithMany(d => d.Ciudades)
                .HasForeignKey(c => c.DepartamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ciudad>()
                .HasIndex(c => new
                {
                    c.DepartamentoId,
                    c.Nombre
                })
                .IsUnique();

            // TIPOS DE AGENCIA
            modelBuilder.Entity<TipoAgencia>()
                .Property(t => t.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<TipoAgencia>()
                .HasIndex(t => t.Nombre)
                .IsUnique();

            // AGENCIAS
            modelBuilder.Entity<Agencia>()
                .Property(a => a.Codigo)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Agencia>()
                .Property(a => a.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Agencia>()
                .Property(a => a.Direccion)
                .HasMaxLength(250);

            modelBuilder.Entity<Agencia>()
                .HasIndex(a => a.Codigo)
                .IsUnique();

            modelBuilder.Entity<Agencia>()
                .HasOne(a => a.Ciudad)
                .WithMany(c => c.Agencias)
                .HasForeignKey(a => a.CiudadId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agencia>()
                .HasOne(a => a.TipoAgencia)
                .WithMany(t => t.Agencias)
                .HasForeignKey(a => a.TipoAgenciaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ESTADOS DE UPS
            modelBuilder.Entity<EstadoUPS>()
                .Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<EstadoUPS>()
                .HasIndex(e => e.Nombre)
                .IsUnique();

            // UPS
            modelBuilder.Entity<UPS>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<UPS>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<UPS>()
                .Property(u => u.NumeroInventario)
                .UseCollation("Latin1_General_100_CI_AS")
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<UPS>()
                .Property(u => u.Marca)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<UPS>()
                .Property(u => u.Modelo)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<UPS>()
                .Property(u => u.NumeroSerie)
                .UseCollation("Latin1_General_100_CI_AS")
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<UPS>()
                .HasIndex(u => u.NumeroInventario)
                .IsUnique();

            modelBuilder.Entity<UPS>()
                .HasIndex(u => u.NumeroSerie)
                .IsUnique();

            modelBuilder.Entity<UPS>()
                .HasOne(u => u.Agencia)
                .WithMany(a => a.UPSs)
                .HasForeignKey(u => u.AgenciaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UPS>()
                .HasOne(u => u.EstadoUPS)
                .WithMany(e => e.UPSs)
                .HasForeignKey(u => u.EstadoUPSId)
                .OnDelete(DeleteBehavior.Restrict);

            // ESTADOS DE MANTENIMIENTO
            modelBuilder.Entity<EstadoMantenimiento>()
                .Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<EstadoMantenimiento>()
                .HasIndex(e => e.Nombre)
                .IsUnique();

            // MANTENIMIENTOS
            modelBuilder.Entity<Mantenimiento>()
                .Property(m => m.Descripcion)
                .HasMaxLength(1000)
                .IsRequired();

            modelBuilder.Entity<Mantenimiento>()
                .Property(m => m.TecnicoProveedor)
                .HasMaxLength(150);

            modelBuilder.Entity<Mantenimiento>()
                .Property(m => m.Observaciones)
                .HasMaxLength(1000);

            modelBuilder.Entity<Mantenimiento>()
                .Property(m => m.AgenciaNombreRegistro)
                .HasMaxLength(150);

            modelBuilder.Entity<Mantenimiento>()
                .HasOne(m => m.UPS)
                .WithMany(u => u.Mantenimientos)
                .HasForeignKey(m => m.UPSId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mantenimiento>()
                .HasOne(m => m.EstadoMantenimiento)
                .WithMany(e => e.Mantenimientos)
                .HasForeignKey(m => m.EstadoMantenimientoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ROLES
            modelBuilder.Entity<Rol>()
                .Property(r => r.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Rol>()
                .Property(r => r.Descripcion)
                .HasMaxLength(250);

            modelBuilder.Entity<Rol>()
                .HasIndex(r => r.Nombre)
                .IsUnique();

            // USUARIOS
            modelBuilder.Entity<Usuario>()
                .Property(u => u.NombreUsuario)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.NombreCompleto)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Correo)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.PasswordHash)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany()
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            // PERMISOS
            modelBuilder.Entity<Permiso>()
                .Property(p => p.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Permiso>()
                .Property(p => p.Codigo)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Permiso>()
                .Property(p => p.Descripcion)
                .HasMaxLength(250);

            modelBuilder.Entity<Permiso>()
                .HasIndex(p => p.Codigo)
                .IsUnique();

            // ROL - PERMISO
            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Rol)
                .WithMany(r => r.RolPermisos)
                .HasForeignKey(rp => rp.RolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolPermisos)
                .HasForeignKey(rp => rp.PermisoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolPermiso>()
                .HasIndex(rp => new
                {
                    rp.RolId,
                    rp.PermisoId
                })
                .IsUnique();

            // CONFIGURACIÓN DE PRESUPUESTO
            modelBuilder.Entity<ConfiguracionPresupuestoBaterias>()
                .ToTable("ConfiguracionPresupuestoBaterias");

            modelBuilder.Entity<ConfiguracionPresupuestoBaterias>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ConfiguracionPresupuestoBaterias>()
                .Property(x => x.Anio)
                .IsRequired();

            modelBuilder.Entity<ConfiguracionPresupuestoBaterias>()
                .Property(x => x.MesesReemplazoLocal)
                .IsRequired();

            modelBuilder.Entity<ConfiguracionPresupuestoBaterias>()
                .Property(x => x.MesesReemplazoForanea)
                .IsRequired();

            modelBuilder.Entity<ConfiguracionPresupuestoBaterias>()
                .Property(x => x.CostoUnitarioBateria)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            modelBuilder.Entity<ConfiguracionPresupuestoBaterias>()
                .Property(x => x.Activo)
                .IsRequired();

            modelBuilder.Entity<ConfiguracionPresupuestoBaterias>()
                .Property(x => x.FechaCreacion)
                .IsRequired();

            modelBuilder.Entity<ConfiguracionPresupuestoBaterias>()
                .HasIndex(x => x.Anio)
                .IsUnique();

            // POLÍTICA GLOBAL DE REEMPLAZO DE BATERÍAS
            modelBuilder.Entity<PoliticaReemplazoBaterias>()
                .ToTable("PoliticaReemplazoBaterias");

            modelBuilder.Entity<PoliticaReemplazoBaterias>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<PoliticaReemplazoBaterias>()
                .Property(x => x.MesesReemplazoLocal)
                .IsRequired();

            modelBuilder.Entity<PoliticaReemplazoBaterias>()
                .Property(x => x.MesesReemplazoForanea)
                .IsRequired();
        }
    }
}
