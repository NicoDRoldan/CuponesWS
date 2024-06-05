using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuponesWS.Models
{
    [Table("Cupones")]
    public class CuponModel
    {
        [Key]
        public int Id_Cupon { get; set; }

        public string? Descripcion { get; set; }

        public decimal PorcentajeDto { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public string TipoCupon { get; set; }

        public string? Url_Imagen { get; set; }

        #region Navegación

        public virtual ICollection<CClienteModel>? Cliente { get; set; }

        public virtual ICollection<CDetalleModel>? Detalle { get; set; }

        public virtual ICollection<CHistorialModel>? Historial { get; set; }

        #endregion
    }
}
