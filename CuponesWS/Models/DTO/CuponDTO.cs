namespace CuponesWS.Models.DTO
{
    public class CuponDTO
    {
        public int Id_Cupon { get; set; }

        public string? Descripcion { get; set; }

        public decimal PorcentajeDTO { get; set; }

        public decimal? ImportePromo { get; set; }

        public DateTime FechaInicio {  get; set; }

        public DateTime FechaFin {  get; set; }

        public string? TipoCupon { get; set; }

        public string? Url_Imagen { get; set; }

        public bool Activo {  get; set; }

        public List<int>? CategoriasSeleccionadas { get; set; }

        public List<int>? ArticulosSeleccionados { get; set; }

        public virtual ICollection<ClienteDTO>? Cliente {  get; set; }

        public virtual ICollection<DetalleDTO>? Detalle { get; set; }

        public virtual ICollection<HistorialDTO>? Historial { get; set; }
    }
}
