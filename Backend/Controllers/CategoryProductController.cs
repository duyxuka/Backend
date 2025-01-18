using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Microsoft.AspNetCore.Cors;

namespace Backend.Controllers
{
    [EnableCors("AddCors")]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryProductController : ControllerBase
    {
        private readonly AppDBContext _context;

        public CategoryProductController(AppDBContext context)
        {
            _context = context;
        }
        // GET: api/CategoryProducts
        [HttpGet]
        public async Task<ActionResult<CategoryProduct>> GetALL()
        {
            var data = await _context.CategoryProducts.ToArrayAsync();

            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string name, int page = 1, int pageSize = 10)
        {
            var categorysQuery = _context.CategoryProducts.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                categorysQuery = categorysQuery.Where(p => p.Name.Contains(name));
            }

            var totalItems = await categorysQuery.CountAsync();

            var categorys = await categorysQuery
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CategoryProductDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatedDate = x.CreatedDate,
                    QuantityProduct = x.Products.Count()
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return Ok(new { categorys, totalPages });
        }

        [HttpGet("countProduct/{id}")]
        public async Task<ActionResult> CountProductInCategory(int id)
        {
            var count = await _context.CategoryProducts
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    QuantityProduct = x.Products.Count()
                })
                .FirstOrDefaultAsync();

            if (count == null)
            {
                return NotFound("Danh mục không tồn tại.");
            }

            return Ok(count);
        }

        [HttpGet("check-name")]
        public async Task<ActionResult<bool>> CheckCategoryName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Tên không hợp lệ.");
            }

            var exists = await _context.CategoryProducts
                .AnyAsync(p => p.Name.ToLower() == name.ToLower());

            return Ok(!exists);
        }

        // GET: api/CategoryProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryProduct>> GetCategoryProduct(int id)
        {
            var categoryProduct = await _context.CategoryProducts.FindAsync(id);

            if (categoryProduct == null)
            {
                return NotFound();
            }

            return categoryProduct;
        }

        // POST: api/CategoryProducts
        [HttpPost]
        public async Task<ActionResult<CategoryProduct>> CreateCategoryProduct(CategoryProduct categoryProduct)
        {
            _context.CategoryProducts.Add(categoryProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryProduct), new { id = categoryProduct.Id }, categoryProduct);
        }

        // PUT: api/CategoryProducts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategoryProduct(int id, CategoryProduct categoryProduct)
        {
            if (id != categoryProduct.Id)
            {
                return BadRequest();
            }

            _context.Entry(categoryProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryProductExists(id))
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

        // DELETE: api/CategoryProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoryProduct(int id)
        {
            var categoryProduct = await _context.CategoryProducts.FindAsync(id);
            if (categoryProduct == null)
            {
                return NotFound();
            }

            _context.CategoryProducts.Remove(categoryProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryProductExists(int id)
        {
            return _context.CategoryProducts.Any(e => e.Id == id);
        }
    }
}
