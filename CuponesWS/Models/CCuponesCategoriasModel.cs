using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CuponesWS.Models
{
    [Table("Cupones_Categorias")]
    public class CCuponesCategoriasModel
    {
        [Key]
        public int Id_Cupones_Categorias { get; set; }

        public int Id_Cupon { get; set; }

        public int Id_Categoria { get; set; }

        [ForeignKey("Id_Cupon")]
        public virtual CuponModel Cupon { get; set; }

        [ForeignKey("Id_Categoria")]
        public virtual CCategoriaModel Categoria { get; set; }
    }
}
