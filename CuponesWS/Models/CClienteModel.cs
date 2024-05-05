using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuponesWS.Models
{
    [Table("Cupones_Clientes")]
    public class CClienteModel
    {
        [Key]
        public int Id_Cupon { get; set; }

        [Key]
        public string NroCupon { get; set; }

        public DateTime FechaAsignado { get; set; }
    }
}
