using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuponesWS.Data;
using CuponesWS.Models;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using System.Text.RegularExpressions;
using System.Text;
using System.Runtime.InteropServices;

namespace CuponesWS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuponesController : ControllerBase
    {
        private readonly CuponesContext _context;

        public CuponesController(CuponesContext context)
        {
            _context = context;
        }

        // GET: api/Cupones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuponModel>>> GetCupones()
        {
            var cupones = new List<CuponModel>();
            try
            {
                cupones = await _context.Cupones
                .Include(c => c.Cliente)
                .Include(c => c.Detalle)
                .Include(c => c.Historial)
                .Include(c => c.Cupones_Categorias)
                    .ThenInclude(cc => cc.Categoria)
                .Where(c => DateTime.Now.Date >= c.FechaInicio && DateTime.Now.Date <= c.FechaFin) // Filtro para traer solo los cupones vigentes.
                .ToListAsync();

                return cupones;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Cupones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CuponModel>> GetCuponModel(int id)
        {
            var cuponModel = await _context.Cupones
                .Include(c => c.Cliente)
                .Include(c => c.Detalle)
                .Include(c => c.Historial)
                .FirstOrDefaultAsync(c => c.Id_Cupon == id);

            if (cuponModel == null)
            {
                return NotFound();
            }

            return cuponModel;
        }

        [HttpPost("CrearCupon")]
        public async Task<ActionResult<CuponModel>> AltaCupon(CuponModel cuponModel)
        {
            try
            {
                _context.Cupones.Add(cuponModel);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return CreatedAtAction("GetCuponModel", new { id = cuponModel.Id_Cupon }, cuponModel);
        }

        [HttpPost("SubirImagenCupon/{Id_Cupon}")]
        public async Task<IActionResult> SubirImagenCupon(string id_Cupon, [FromForm] IFormFile imagen)
        {
            try
            {
                var cupon = await _context.Cupones
                .Where(c => c.Id_Cupon == int.Parse(id_Cupon))
                .FirstOrDefaultAsync();

                if (cupon != null)
                {
                    var uploadsFolder = "C:\\Repositorio\\Proyecto MVC\\PedidosApp\\PedidosApp\\wwwroot\\images\\cupones";
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imagen.FileName;
                    var path = Path.Combine(uploadsFolder, uniqueFileName);

                    if (!Directory.Exists(uploadsFolder))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }
                    cupon.Url_Imagen = "/images/cupones/" + uniqueFileName;

                    _context.Update(cupon);
                    await _context.SaveChangesAsync();

                    return Ok("Imagen guardada correctamente");
                }
                else
                {
                    return BadRequest("Error que se yo tengo sueño");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CrearCuponCliente")]
        public async Task<ActionResult<CClienteModel>> AltaCuponDeCliente(CClienteModel CClienteModel)
        {
            CClienteModel.NroCupon = CClienteModel.GenerarNumeroCupon();

            var nroCuponBD = await _context.CuponesClientes
                .Where(c => c.NroCupon.Equals(CClienteModel.NroCupon))
                .FirstOrDefaultAsync();

            while (nroCuponBD != null)
            {
                CClienteModel.NroCupon = CClienteModel.GenerarNumeroCupon();
            }

            CClienteModel.FechaAsignado = DateTime.Now;

            _context.CuponesClientes.Add(CClienteModel);
            await _context.SaveChangesAsync();

            return Ok(CClienteModel);
        }

        [HttpPost("QuemarCupon")]
        public async Task<IActionResult> QuemarCupon([FromBody] string nroCupon)
        {
            var cuponCliente = await _context.CuponesClientes
                .Where(c => c.NroCupon.Equals(nroCupon))
                .FirstOrDefaultAsync();

            if (cuponCliente == null)
            {
                return BadRequest("El cupón no existe");
            }
            try
            {
                _context.Remove(cuponCliente);

                var cuponHistorico = new CHistorialModel
                {
                    Id_Cupon = cuponCliente.Id_Cupon,
                    NroCupon = cuponCliente.NroCupon,
                    FechaUso = DateTime.Now,
                    CodCliente = cuponCliente.CodCliente
                };
                _context.CuponesHistorial.Add(cuponHistorico);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }

            return Ok();
        }

        [HttpGet("Cupon/{nroCupon}")]
        public async Task<IActionResult> ValidarCupon(string nroCupon)
        {
            Regex regex = new Regex(@"^\d{3}-\d{3}-\d{3}$");
            if (!regex.IsMatch(nroCupon))
            {
                return BadRequest("El número de cupón debe tener el formato xxx-xxx-xxx.");
            }

            var clienteCupon = await _context.CuponesClientes
                .Where(c => c.NroCupon == nroCupon)
                .FirstOrDefaultAsync();

            var histCupon = await _context.CuponesHistorial
                .FirstOrDefaultAsync(ch => ch.NroCupon == nroCupon);

            if (clienteCupon == null)
            {
                if (histCupon is not null) return BadRequest("El cupón ya fue utilizado");

                return BadRequest("El cupón no es valido");
            }

            var cupon = await _context.Cupones
                .Include(c => c.Cliente)
                .Include(c => c.Detalle)
                .Where(c => c.Id_Cupon == clienteCupon.Id_Cupon)
                .FirstOrDefaultAsync();

            if (cupon.FechaInicio >= DateTime.Now || cupon.FechaFin <= DateTime.Now)
            {
                return BadRequest("El cupon no entró en vigencia o esta vencido");
            }

            var cuponJson = await _context.Cupones
                .Include(c => c.Detalle)
                .Where(c => c.Id_Cupon == clienteCupon.Id_Cupon)
                .Select(ed => new
                {
                    Id_Cupon = ed.Id_Cupon,
                    PorcentajeDto = ed.PorcentajeDto,
                    Detalle = ed.Detalle.Select(d => new
                    {
                        Id_Cupon = d.Id_Cupon,
                        Id_ArticuloAsociado = d.Id_ArticuloAsociado,
                        Cantidad = d.Cantidad
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return Ok(cuponJson);
        }
    }
}