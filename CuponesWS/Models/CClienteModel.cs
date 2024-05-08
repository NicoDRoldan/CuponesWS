using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CuponesWS.Models
{
    [Table("Cupones_Clientes")]
    public class CClienteModel
    {
        [Key]
        public int Id_Cupon { get; set; }

        [Key]
        public string? NroCupon { get; set; }

        public DateTime? FechaAsignado { get; set; }

        public string CodCliente { get; set; }

        public string GenerarNumeroCupon()
        {
            Random random = new Random();
            int[] numeros = Enumerable.Range(0, 10).ToArray(); // Array con los números del 0 al 9
            StringBuilder cuponBuilder = new StringBuilder();

            // Generar los primeros tres dígitos
            for (int i = 0; i < 3; i++)
            {
                int numeroAleatorio = numeros[random.Next(0, numeros.Length)]; // Obtener un número aleatorio del array
                cuponBuilder.Append(numeroAleatorio.ToString());
            }

            // Añadir el guion
            cuponBuilder.Append("-");

            // Generar los siguientes tres dígitos
            for (int i = 0; i < 3; i++)
            {
                int numeroAleatorio = numeros[random.Next(0, numeros.Length)]; // Obtener un número aleatorio del array
                cuponBuilder.Append(numeroAleatorio.ToString());
            }

            // Añadir el guion
            cuponBuilder.Append("-");

            // Generar los últimos tres dígitos
            for (int i = 0; i < 3; i++)
            {
                int numeroAleatorio = numeros[random.Next(0, numeros.Length)]; // Obtener un número aleatorio del array
                cuponBuilder.Append(numeroAleatorio.ToString());
            }

            return cuponBuilder.ToString();
        }

    }


}
