using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Grocery.WebApp.Data.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grocery.WebApp.Models
{
    public class MyIdentityUser
        : IdentityUser<Guid>
    {
        [Required]                                  // NOT NULL constraint for DB Schema
        [Display(Name = "Display Name")]             // constraint for Razor UI Label
        [MinLength(2)]                              // constraint for UI
        [MaxLength(60)]                             // constraint for UI
        [StringLength(60)]                          // constraint for DB Schema
        public string DisplayName { get; set; }

        [Required]
        [Display(Name = "Gender")]
        [PersonalData]                              // for GDPR Compliance
        public MyAppGenderTypes Gender { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [PersonalData]
        [Column(TypeName = "smalldatetime")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Is Admin User?")]
        public bool IsAdminUser { get; set; }

        [Display(Name = "Photo")]
        public byte[] Photo { get; set; }


        #region Navigation Properties

        [ForeignKey(nameof(Product.CreatedByUserId))]
        public ICollection<Product> ProductsCreatedByUser { get; set; }

        [ForeignKey(nameof(Product.UpdatedByUserId))]
        public ICollection<Product> ProductsUpdatedByUser { get; set; }

        #endregion
    }
}
