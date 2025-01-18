using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Product")]
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public float Price { get; set; }
        public int CategoryProductId { get; set; }
        public CategoryProduct CategoryProduct { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ProductDTO : Product
    {
        public string CategoryProductName { get; set; }
    }
}
