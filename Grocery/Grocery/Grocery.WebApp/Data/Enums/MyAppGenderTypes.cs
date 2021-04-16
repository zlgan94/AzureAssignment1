using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace Grocery.WebApp.Data.Enums
{
    public enum MyAppGenderTypes
    {
        [Display(Name = "Male")]
        Male,

        [Display(Name = "Female")]
        Female,

        [Display(Name = "Third Gender")]
        ThirdGender
    }
}
