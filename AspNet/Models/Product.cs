using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductServer.Models
{
    public class Product : IProduct
    {
        [Key]
        public Guid ID { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; }

        public DateTime? DiscontinuedDate { get; set; }

        [Required]
        public Int16 Rating { get; set; }

        [Required]
        public double Price { get; set; }

        // Relations
        public Guid ProductDetailID { get; set; }
        public ProductDetail ProductDetail { get; set; }

        public Guid SupplierID { get; set; }
        public Supplier Supplier { get; set; }

        public List<Category> Categories { get; set; }
    }

    interface IProduct
    {
        Guid ID { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        DateTime ReleaseDate { get; set; }

        DateTime? DiscontinuedDate { get; set; }

        Int16 Rating { get; set; }

        double Price { get; set; }

        ProductDetail ProductDetail { get; set; }

        Supplier Supplier { get; set; }

        List<Category> Categories { get; set; }
    }
}
