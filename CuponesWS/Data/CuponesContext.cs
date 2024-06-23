using CuponesWS.Models;
using Microsoft.EntityFrameworkCore;

namespace CuponesWS.Data
{
    public class CuponesContext : DbContext
    {
        public CuponesContext(DbContextOptions<CuponesContext> options) : base(options) { }

        public DbSet<CuponesWS.Models.CuponModel> Cupones { get; set; }
        public DbSet<CuponesWS.Models.CClienteModel> CuponesClientes { get; set; }
        public DbSet<CuponesWS.Models.CDetalleModel> CuponesDetalles { get; set; }
        public DbSet<CuponesWS.Models.CHistorialModel> CuponesHistorial { get; set; }
        public DbSet<CuponesWS.Models.CCategoriaModel> CuponesCategorias { get; set; } // Modelo de Categorias
        public DbSet<CuponesWS.Models.CCuponesCategoriasModel> Cupones_Categorias { get; set; } // Relación de muchos a muchos

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CClienteModel>()
                .HasKey(cc => new { cc.Id_Cupon, cc.NroCupon });
            modelBuilder.Entity<CDetalleModel>()
                .HasKey(cd => new { cd.Id_Cupon, cd.Id_ArticuloAsociado });
            modelBuilder.Entity<CHistorialModel>()
                .HasKey(ch => new { ch.Id_Cupon, ch.NroCupon });

            modelBuilder.Entity<CCuponesCategoriasModel>()
                .HasOne(cc => cc.Cupon)
                .WithMany(cc => cc.Cupones_Categorias)
                .HasForeignKey(cc => cc.Id_Cupon);

            base.OnModelCreating(modelBuilder);
        }
    }
}
