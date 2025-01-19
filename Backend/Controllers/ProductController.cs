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
    public class ProductController : ControllerBase
    {
        private readonly AppDBContext _context;

        public ProductController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<Product>> GetALL()
        {
            var data = await _context.Products.AsNoTracking().ToArrayAsync();

            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string name, int? categoryId, int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page và PageSize phải lớn hơn 0.");
            }

            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.CategoryProduct)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var sanitizedName = name.Trim().ToLower();
                query = query.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{sanitizedName}%"));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryProductId == categoryId.Value);
            }

            var totalItems = await query.CountAsync();
            if (totalItems == 0)
            {
                return Ok(new { products = new List<ProductDTO>(), totalPages = 0 });
            }

            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryProductName = p.CategoryProduct.Name,
                    CreatedDate = p.CreatedDate
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return Ok(new { products, totalPages });
        }

        [HttpGet("check-name")]
        public async Task<IActionResult> CheckProductName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Tên sản phẩm không hợp lệ.");
            }

            var sanitizedName = name.Trim();
            var exists = await _context.Products
                .AsNoTracking()
                .AnyAsync(p => p.Name.Equals(sanitizedName, StringComparison.OrdinalIgnoreCase));

            return Ok(!exists);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.CategoryProduct)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {

             _context.Products.Add(product);
             await _context.SaveChangesAsync();

             return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("ID sản phẩm không tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _context.Products.AnyAsync(p => p.Id == id))
            {
                return NotFound();
            }
               
            _context.Entry(product).State = EntityState.Modified;

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
