using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD4430_HSZF_2024252.Model
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[ForeignKey("FavoriteItem")]
        public int Id { get; set; }



        [StringLength(100)]
        [Required]
        public required string Name { get; set; }


        [Column(TypeName = "decimal(15,1)")]
        [Required]
        public decimal Quantity { get; set; }


        [Column(TypeName = "decimal(15,1)")]
        [Required]
        public decimal CriticalLevel { get; set; }


        [DataType(DataType.Date)]
        [Required]
        public DateTime BestBefore { get; set; }

        [Required]
        public bool StoreInFridge { get; set; }

    }
    public class FavoriteProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int personId { get; set; }
        [Required]
        public int productId { get; set; }
    }
    public class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[ForeignKey("FavoriteItem")]
        public int Id { get; set; }

        [StringLength(100)]
        [Required]
        public required string Name { get; set; }

        [Required]
        public bool ResponsibleForPurchase { get; set; }
    }

    public class StorageUnit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = null;

        [Required]
        [Column(TypeName = "decimal(15,1)")]
        public decimal Capacity { get; set; }
    }
}
