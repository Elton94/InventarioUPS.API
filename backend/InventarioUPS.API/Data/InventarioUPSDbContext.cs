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

        // =====================================================
        // TABLAS
        // =====================================================

        public DbSet<Permiso> Permisos { get; set; }

        public DbSet<RolPermiso> RolPermisos { get; set; }

        public DbSet<Agencia> Agencias { get; set; }

        public DbSet<Departamento> Departamentos { get; set; }

        public DbSet<Ciudad> Ciudades { get; set; }

        public DbSet<TipoAgencia> TiposAgencia { get; set; }

        public DbSet<UPS> UPSs { get; set; }

        public DbSet<EstadoUPS> EstadosUPS { get; set; }

        public DbSet<Mantenimiento> Mantenimientos { get; set; }

        public DbSet<TipoMantenimiento> TiposMantenimiento { get; set; }

        public DbSet<EstadoMantenimiento> EstadosMantenimiento { get; set; }

        public DbSet<Rol> Roles { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<ConfiguracionPresupuestoBaterias>
            ConfiguracionPresupuestoBaterias
        { get; set; }

        public DbSet<PoliticaReemplazoBaterias>
            PoliticaReemplazoBaterias
        { get; set; }


        // =====================================================
        // CONFIGURACIÓN
        // =====================================================

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =================================================
            // DEPARTAMENTOS
            // =================================================

            modelBuilder.Entity<Departamento>()
                .Property(d => d.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Departamento>()
                .HasIndex(d => d.Nombre)
                .IsUnique();


            // =================================================
            // CIUDADES
            // =================================================

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


            // =================================================
            // TIPOS DE AGENCIA
            // =================================================

            modelBuilder.Entity<TipoAgencia>()
                .Property(t => t.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<TipoAgencia>()
                .HasIndex(t => t.Nombre)
                .IsUnique();


            // =================================================
            // AGENCIAS
            // =================================================

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


            // =================================================
            // ESTADOS DE UPS
            // =================================================

            modelBuilder.Entity<EstadoUPS>()
                .Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<EstadoUPS>()
                .HasIndex(e => e.Nombre)
                .IsUnique();


            // =================================================
            // UPS
            // =================================================

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


            // =================================================
            // TIPOS DE MANTENIMIENTO
            // =================================================

            modelBuilder.Entity<TipoMantenimiento>()
                .Property(t => t.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<TipoMantenimiento>()
                .HasIndex(t => t.Nombre)
                .IsUnique();


            // =================================================
            // ESTADOS DE MANTENIMIENTO
            // =================================================

            modelBuilder.Entity<EstadoMantenimiento>()
                .Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<EstadoMantenimiento>()
                .HasIndex(e => e.Nombre)
                .IsUnique();


            // =================================================
            // MANTENIMIENTOS
            // =================================================

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
                .Property(m => m.Costo)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Mantenimiento>()
                .Property(m => m.CostoBaterias)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Mantenimiento>()
                .Property(m => m.AgenciaNombreRegistro)
                .HasMaxLength(150);


            // UPS → Mantenimientos

            modelBuilder.Entity<Mantenimiento>()
                .HasOne(m => m.UPS)
                .WithMany(u => u.Mantenimientos)
                .HasForeignKey(m => m.UPSId)
                .OnDelete(DeleteBehavior.Restrict);


            // Tipo → Mantenimientos

            modelBuilder.Entity<Mantenimiento>()
                .HasOne(m => m.Tipo)
                .WithMany(t => t.Mantenimientos)
                .HasForeignKey(m => m.TipoMantenimientoId)
                .OnDelete(DeleteBehavior.Restrict);


            // Estado → Mantenimientos

            modelBuilder.Entity<Mantenimiento>()
                .HasOne(m => m.EstadoMantenimiento)
                .WithMany(e => e.Mantenimientos)
                .HasForeignKey(m => m.EstadoMantenimientoId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // ROLES
            // =================================================

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


            // =================================================
            // USUARIOS
            // =================================================

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


            // =================================================
            // PERMISOS
            // =================================================

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


            // =================================================
            // ROL - PERMISO
            // =================================================

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


            // =================================================
            // CONFIGURACIÓN DE PRESUPUESTO
            // =================================================

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

            // =================================================
            // POLÍTICA GLOBAL DE REEMPLAZO DE BATERÍAS
            // =================================================

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
