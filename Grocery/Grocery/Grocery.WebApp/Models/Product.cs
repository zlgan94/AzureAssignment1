using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Grocery.WebApp.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [Required]
        [Column(name: "ProductId")]                             // name of the column in the DB
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   // to indicate DB constraint
        [Comment("The Unique ID of the Product.")]              // DB Schema related documentation
        public Guid ProductID { get; set; }

        [Required]
        [StringLength(80)]
        [Column(TypeName = "varchar")]
        [Comment("The Name of the Product sold by the Store.")]
        public string ProductName { get; set; }

        [Required]
        [Comment("The Quantity of the Product currently available in the Store.")]
        // [DefaultValue(0)] - done using the OnModelCreating()
        public short Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal SellingPricePerUnit { get; set; }

        [Comment("The Image of the Product.")]
        public byte[] Image { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        [Required]
        public DateTime LastUpdatedOn { get; set; }


        #region Navigation Properties 

        public MyIdentityUser CreatedByUser { get; set; }
        
        public MyIdentityUser UpdatedByUser { get; set; }
        
        #endregion

    }
}
