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
            return await _context.Cupones
                .Include(c => c.Cliente)
                .Include(c => c.Detalle)
                .Include(c => c.Historial)
                .ToListAsync();
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
        public async Task<ActionResult<CuponModel>> PostCuponModel(CuponModel cuponModel)
        {
            _context.Cupones.Add(cuponModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCuponModel", new { id = cuponModel.Id_Cupon }, cuponModel);
        }

        [HttpPost("RecibirSolicitud")]
        public async Task<ActionResult> RecibirSolicitudCupon([FromBody] object json)
        {
            return Ok(json);
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

            if (clienteCupon == null)
            {
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
                        Id_ArticuloAsociado = d.Id_ArticuloAsociado
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return Ok(cuponJson);
        }
    }
}
