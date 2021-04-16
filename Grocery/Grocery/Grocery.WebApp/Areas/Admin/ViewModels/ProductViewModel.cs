using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Grocery.WebApp.Models;

namespace Grocery.WebApp.Areas.Admin.ViewModels
{
    public class ProductViewModel
    {
        [Required]
        [Display(Name = "Product ID")]
        public Guid ProductID { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(80)]
        [Display(Name = "Product name")]
        public string ProductName { get; set; }

        [Required]
        [Display(Name = "Quantity")]
        public short Quantity { get; set; }

        [Required]
        [Display(Name = "Selling Price per Unit")]
        public decimal SellingPricePerUnit { get; set; }

        // Property to store the image received from the database
        public byte[] Image { get; set; }

        // Property to receive the file uploaded for the Image!
        [Display(Name = "Image for the Product")]
        public IFormFile ImageFile { get; set; }


        [Required]
        [Display(Name = "Created by")]
        public Guid CreatedByUserId { get; set; }

        [Display(Name = "Updated by")]
        public Guid? UpdatedByUserId { get; set; }

        [Required]
        [Display(Name = "Last updated on")]
        public DateTime LastUpdatedOn { get; set; }


        #region Navigation Properties 

        public MyIdentityUser CreatedByUser { get; set; }

        public MyIdentityUser UpdatedByUser { get; set; }

        #endregion

    }
}
