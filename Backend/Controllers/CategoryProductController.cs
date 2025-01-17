using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Controllers
{
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
        public async Task<IActionResult> GetCategoryProducts(int page = 1, int pageSize = 10)
        {
            var products = await _context.CategoryProducts.Select(x => new CategoryProductDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatedDate = x.CreatedDate,
                    QuantityProduct = x.Products.Count()
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalProducts = await _context.CategoryProducts.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            return Ok(new
            {
                products,
                totalPages
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string name)
        {
            // Lọc sản phẩm theo tên và category
            var productsQuery = _context.CategoryProducts.AsQueryable();

            // Nếu có tham số tìm kiếm tên, lọc theo tên
            if (!string.IsNullOrEmpty(name))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(name));
            }

            // Lấy danh sách sản phẩm và trả về
            var products = await productsQuery  // Nếu bạn muốn lấy thông tin category
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name, // Assuming Category has Name
                    CreatedDate = p.CreatedDate
                })
                .ToListAsync();

            return Ok(products);
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
