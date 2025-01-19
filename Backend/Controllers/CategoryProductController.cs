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
            var data = await _context.CategoryProducts.AsNoTracking().ToListAsync();

            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string name, int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page và PageSize phải lớn hơn 0.");
            }  

            var query = _context.CategoryProducts.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var sanitizedName = name.Trim().ToLower();
                query = query.Where(cp => EF.Functions.Like(cp.Name.ToLower(), $"%{sanitizedName}%"));
            }

            var totalItems = await query.CountAsync();
            if (totalItems == 0)
            {
                return Ok(new { categorys = new List<CategoryProductDTO>(), totalPages = 0 });
            }
            var categorys = await query
                .OrderByDescending(cp => cp.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cp => new CategoryProductDTO
                {
                    Id = cp.Id,
                    Name = cp.Name,
                    CreatedDate = cp.CreatedDate,
                    QuantityProduct = cp.Products.Count()
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
                return NotFound("Loại sản phẩm không tồn tại.");
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
            var sanitizedName = name.Trim();
            var exists = await _context.Products
                .AsNoTracking()
                .AnyAsync(cp => cp.Name.Equals(sanitizedName, StringComparison.OrdinalIgnoreCase));

            return Ok(!exists);
        }

        // GET: api/CategoryProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryProduct>> GetCategoryProduct(int id)
        {
            var categoryProduct = await _context.CategoryProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

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
                return BadRequest("Id không tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _context.CategoryProducts.AnyAsync(cp => cp.Id == id))
            {
                return NotFound();
            }  

            _context.Entry(categoryProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Xuất hiện lỗi khi cập nhật.");
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
