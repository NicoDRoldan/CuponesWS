using CuponesWS.Data;
using CuponesWS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                var categorias = await _context.CuponesCategorias
                    .ToListAsync();

                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
