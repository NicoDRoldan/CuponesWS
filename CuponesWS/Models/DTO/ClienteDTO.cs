namespace CuponesWS.Models.DTO
{
    public class ClienteDTO
    {
        public int Id_cupon { get; set; }

        public string NroCupon { get; set; }

        public DateTime? FechaAsignado { get; set; }

        public string CodCliente { get; set; }
    }
}
