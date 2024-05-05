using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuponesWS.Models
{
    [Table("Cupones_Historial")]
    public class CHistorialModel
    {
        [Key]
        public int Id_Cupon { get; set; }

        [Key]
        public string NroCupon { get; set; }

        public DateTime FechaUso { get; set; }

        public string CodCliente { get; set; }
    }
}
