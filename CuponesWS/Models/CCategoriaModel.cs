using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuponesWS.Models
{
    [Table("Categorias")]
    public class CCategoriaModel
    {
        [Key]
        public int Id_Categoria { get; set; }

        public string Nombre { get; set; }
    }
}
