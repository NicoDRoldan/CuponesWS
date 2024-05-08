using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuponesWS.Models
{
    [Table("Cupones_Detalle")]
    public class CDetalleModel
    {
        [Key]
        public int Id_Cupon { get; set; }

        [Key]
        public int Id_ArticuloAsociado { get; set; }

        public int Cantidad { get; set; }
    }
}
