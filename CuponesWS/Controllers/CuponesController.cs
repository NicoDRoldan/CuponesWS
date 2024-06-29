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
using CuponesWS.Models.DTO;

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
                //.Where(c => (DateTime.Now.Date >= c.FechaInicio && DateTime.Now.Date <= c.FechaFin)
                //    && c.Activo == true) // Filtro para traer solo los cupones vigentes.
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
        public async Task<ActionResult<CuponDTO>> GetCuponModel(int id)
        {
            var cuponModel = await _context.Cupones
                .Include(c => c.Cliente)
                .Include(c => c.Detalle)
                .Include(c => c.Historial)
                .Include(c => c.Cupones_Categorias)
                .Where(c => c.Id_Cupon == id)
                .Select(c => new CuponDTO
                {
                    Id_Cupon = c.Id_Cupon,
                    Descripcion = c.Descripcion,
                    PorcentajeDTO = c.PorcentajeDto,
                    FechaInicio = c.FechaInicio,
                    FechaFin = c.FechaFin,
                    TipoCupon = c.TipoCupon,
                    Url_Imagen = c.Url_Imagen,
                    Activo = c.Activo,
                    CategoriasSeleccionadas = c.Cupones_Categorias
                                                .Select(cc => cc.Id_Categoria).ToList(),
                    ArticulosSeleccionados = c.Detalle
                                                .Select(cd => cd.Id_ArticuloAsociado).ToList(),
                    Cliente = c.Cliente.Select(cliente => new ClienteDTO
                    {
                        Id_cupon = cliente.Id_Cupon,
                        NroCupon = cliente.NroCupon,
                        FechaAsignado = cliente.FechaAsignado,
                        CodCliente = cliente.CodCliente
                    }).ToList(),
                    Detalle = c.Detalle.Select(detalle => new DetalleDTO
                    {
                        Id_Cupon = detalle.Id_Cupon,
                        Id_ArticuloAsociado = detalle.Id_ArticuloAsociado,
                        Cantidad = detalle.Cantidad
                    }).ToList(),
                    Historial = c.Historial.Select(historial => new HistorialDTO
                    {
                        Id_Cupon = historial.Id_Cupon,
                        NroCupon = historial.NroCupon,
                        FechaUso = historial.FechaUso,
                        CodCliente = historial.CodCliente
                    }).ToList()
                })
                .FirstOrDefaultAsync();

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

                if (cuponModel.CategoriasSeleccionadas.Any())
                {
                    foreach (var id_Categoria in cuponModel.CategoriasSeleccionadas)
                    {
                        var cupones_Categorias = new CCuponesCategoriasModel
                        {
                            Id_Cupon = cuponModel.Id_Cupon,
                            Id_Categoria = id_Categoria
                        };
                        _context.Cupones_Categorias.Add(cupones_Categorias);
                    }
                    await _context.SaveChangesAsync();
                }
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
                    if(imagen != null)
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
                    }
                    else
                    {
                        cupon.Url_Imagen = await _context.Cupones.AsNoTracking().Where(c => c.Id_Cupon == int.Parse(id_Cupon)).Select(c => c.Url_Imagen).FirstOrDefaultAsync();
                    }

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cupon = await _context.Cupones.FirstOrDefaultAsync(c => c.Id_Cupon == id);

                if (cupon != null)
                {
                    var cupon_Cliente = await _context.CuponesClientes.Where(c => c.Id_Cupon == id).ToListAsync();

                    if (cupon_Cliente.Any()) return BadRequest("No se puede eliminar el cupón debido a que un cliente tiene el cupón vigente");

                    var cupon_Detalle = await _context.CuponesDetalles.Where(c => c.Id_Cupon == id).ToListAsync();

                    if (cupon_Detalle.Any()) _context.CuponesDetalles.RemoveRange(cupon_Detalle);

                    var categorias_Cupones = await _context.Cupones_Categorias.Where(c => c.Id_Cupon == id).ToListAsync();

                    if (categorias_Cupones.Any()) _context.Cupones_Categorias.RemoveRange(categorias_Cupones);
                    

                    _context.Cupones.Remove(cupon);

                    await _context.SaveChangesAsync();

                    return Ok("Cupón eliminado correctamente");
                }
                else
                {
                    return BadRequest("El cupón no existe.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, CuponModel cuponModel)
        {
            try
            {
                var cupon = await _context.Cupones
                    .Include(c => c.Detalle)
                    .Where(c => c.Id_Cupon == id)
                    .FirstOrDefaultAsync() ?? throw new Exception("El cupón no existe");

                cupon.Descripcion = cuponModel.Descripcion;
                cupon.PorcentajeDto = cuponModel.PorcentajeDto;
                cupon.FechaInicio = cuponModel.FechaInicio;
                cupon.FechaFin = cuponModel.FechaFin;
                cupon.TipoCupon = cuponModel.TipoCupon;
                cupon.Url_Imagen = cuponModel.Url_Imagen;
                cupon.Activo = cuponModel.Activo;

                // Edición de Detalle de Cupón
                var cupones_detallesExistentes = await _context.CuponesDetalles.Where(cd => cd.Id_Cupon == id).ToListAsync();

                if (cuponModel.Detalle != null)
                {
                    if (cuponModel.Detalle.Any())
                    {
                        _context.RemoveRange(cupones_detallesExistentes);

                        foreach(var detalle in cuponModel.Detalle)
                        {
                            CDetalleModel cDetalleModel = new CDetalleModel
                            {
                                Id_Cupon = id,
                                Id_ArticuloAsociado = detalle.Id_ArticuloAsociado,
                                Cantidad = detalle.Cantidad
                            };
                            _context.Add(cDetalleModel);
                        }
                    }
                }
                else
                {
                    _context.RemoveRange(cupones_detallesExistentes);
                }

                // Edición de Categorias
                List<int> idTotalCategorias = await _context.CuponesCategorias.Select(cc => cc.Id_Categoria).ToListAsync();
                var cupones_CategoriasExistentes = await _context.Cupones_Categorias.Where(c => c.Id_Cupon == id).ToListAsync();

                if (cuponModel.CategoriasSeleccionadas != null)
                {
                    if (cuponModel.CategoriasSeleccionadas.Any())
                    {
                        foreach (var id_CategoriaSeleccionada in cuponModel.CategoriasSeleccionadas)
                        {
                            foreach (var id_Categoria in idTotalCategorias)
                            {
                                var cuponCategoriaExt = await _context.Cupones_Categorias
                                    .Where(cc => cc.Id_Cupon == id && cc.Id_Categoria == id_Categoria)
                                    .FirstOrDefaultAsync();

                                if (id_Categoria == id_CategoriaSeleccionada && cuponCategoriaExt is null)
                                {
                                    CCuponesCategoriasModel cupones_categorias = new CCuponesCategoriasModel
                                    {
                                        Id_Cupon = id,
                                        Id_Categoria = id_Categoria,
                                    };
                                    _context.Add(cupones_categorias);
                                }
                                else if (!cuponModel.CategoriasSeleccionadas.Contains(id_Categoria) && cuponCategoriaExt != null)
                                {
                                    _context.Remove(cuponCategoriaExt);
                                }
                            }
                        }
                    }
                }
                else
                {
                    _context.RemoveRange(cupones_CategoriasExistentes);
                }

                _context.Update(cupon);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}