using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuponesWS.Data;
using CuponesWS.Models;

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
            var cuponModel = await _context.Cupones.FindAsync(id);

            if (cuponModel == null)
            {
                return NotFound();
            }

            return cuponModel;
        }

        // PUT: api/Cupones/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCuponModel(int id, CuponModel cuponModel)
        {
            if (id != cuponModel.Id_Cupon)
            {
                return BadRequest();
            }

            _context.Entry(cuponModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CuponModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Cupones
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CuponModel>> PostCuponModel(CuponModel cuponModel)
        {
            _context.Cupones.Add(cuponModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCuponModel", new { id = cuponModel.Id_Cupon }, cuponModel);
        }

        // DELETE: api/Cupones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCuponModel(int id)
        {
            var cuponModel = await _context.Cupones.FindAsync(id);
            if (cuponModel == null)
            {
                return NotFound();
            }

            _context.Cupones.Remove(cuponModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CuponModelExists(int id)
        {
            return _context.Cupones.Any(e => e.Id_Cupon == id);
        }
    }
}
