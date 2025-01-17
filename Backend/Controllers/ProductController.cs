using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Controllers
{
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
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
        {
            var products = await _context.Products.AsNoTracking().Join(_context.CategoryProducts, p => p.CategoryId, c => c.Id,
                (p, c) => new { p, c }).Select(x => new ProductDTO
                {
                    Id = x.p.Id,
                    Name = x.p.Name,
                    Price = x.p.Price,
                    CategoryProductName = x.c.Name,
                    CreatedDate = x.p.CreatedDate,
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalProducts = await _context.Products.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            return Ok(new
            {
                products,
                totalPages
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string name, int? categoryId)
        {
            // Lọc sản phẩm theo tên và category
            var productsQuery = _context.Products.AsQueryable();

            // Nếu có tham số tìm kiếm tên, lọc theo tên
            if (!string.IsNullOrEmpty(name))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(name));
            }

            // Nếu có tham số categoryId, lọc theo categoryId
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            // Lấy danh sách sản phẩm và trả về
            var products = await productsQuery
                .Include(p => p.CategoryProduct)  // Nếu bạn muốn lấy thông tin category
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryProductName = p.CategoryProduct.Name,  // Assuming Category has Name
                    CreatedDate = p.CreatedDate
                })
                .ToListAsync();

            return Ok(products);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("Product ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
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
    }
}
