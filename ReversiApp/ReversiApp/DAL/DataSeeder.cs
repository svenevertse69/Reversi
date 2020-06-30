using Microsoft.AspNetCore.Identity;
using ReversiApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiApp.DAL
{
    public static class DataSeeder
    {
        public static void SeedData(UserManager<Speler> userManager, RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
        }

        public static void SeedUsers(UserManager<Speler> userManager)
        {
            if (userManager.FindByNameAsync("admin").Result == null)
            {
                Speler user = new Speler();
                user.UserName = "admin";
                user.Email = "admin@admin.com";
                user.EmailConfirmed = true;
                user.Token = Guid.NewGuid().ToString();
                user.Naam = "admin";
                IdentityResult result = userManager.CreateAsync(user, "Test123").Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }
            }
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync("Speler").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Speler";
                role.NormalizedName = "SPELER";
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }

            if (!roleManager.RoleExistsAsync("Mod").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Mod";
                role.NormalizedName = "MOD";
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }

            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Admin";
                role.NormalizedName = "ADMIN";
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }
        }
    }
}