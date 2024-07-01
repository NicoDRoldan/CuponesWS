using CuponesWS.Data;
using CuponesWS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace CuponesWS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly CuponesContext _context;

        public CategoriaController(CuponesContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CCategoriaModel>>> GetCategorias()
        {
            try
            {
                return Ok(await _context.CuponesCategorias
                    .ToListAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CrearCategoria")]
        public async Task<ActionResult> Create(CCategoriaModel cCategoriaModel)
        {
            try
            {
                _context.CuponesCategorias.Add(cCategoriaModel);
                await _context.SaveChangesAsync();

                var message = $"Categoria {cCategoriaModel.Nombre} creada correctamente.";
                var response = new
                {
                    Message = message,
                    Categoria = cCategoriaModel
                };

                return CreatedAtAction("GetCategorias", new { id = cCategoriaModel.Id_Categoria }, response);
            }
            catch(Exception ex)
            {
                return BadRequest($"Error al crear la categoria: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Traer la categoría por id
                var categoria = await _context.CuponesCategorias
                    .FirstOrDefaultAsync(c => c.Id_Categoria == id) 
                    ?? throw new Exception("Error, la categoria no existe.");

                // Traer los cupones asociados a la categoría
                var categoriasCupones = await _context.Cupones_Categorias
                    .Where(cc => cc.Id_Categoria == id)
                    .ToListAsync();

                if(categoriasCupones.Any())
                    _context.RemoveRange(categoriasCupones);

                _context.Remove(categoria);
                await _context.SaveChangesAsync();

                return Ok("Categoria eliminada correctamente.");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
