using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Grocery.WebApp.Models;
using Grocery.WebApp.Data.Enums;

namespace Grocery.WebApp.Data
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedRolesAsync(RoleManager<MyIdentityRole> roleManager)
        {
            //MyIdentityRole role = new MyIdentityRole()
            //{
            //     Name = "Administrator",
            //     Description = "The Administrator for the Application"
            //};
            //await roleManager.CreateAsync(role);

            foreach(MyAppRoleTypes role in Enum.GetValues(typeof(MyAppRoleTypes)) )
            {
                MyIdentityRole roleObj = new MyIdentityRole()
                {
                    Name = role.ToString(),
                    Description = $"The {role} for the Application"
                };
                await roleManager.CreateAsync(roleObj);
            }
        }
    }
}
