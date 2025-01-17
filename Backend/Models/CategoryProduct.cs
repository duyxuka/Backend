using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("CategoryProduct")]
    public class CategoryProduct
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
    public class CategoryProductDTO : CategoryProduct
    {
        public int QuantityProduct { get; set; }
    }
}
