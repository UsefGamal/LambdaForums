using LambdaForums.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LambdaForums.Data
{
    public class DataSeeder
    {
        private ApplicationDbContext _context; 
        public DataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public  Task SeedSuperUser()
        {
            var roleStore = new RoleStore<IdentityRole>(_context);
            var userStore = new UserStore<ApplicationUser>(_context);


            var user = new ApplicationUser
            {
                UserName = "ForumAdmin",
                NormalizedUserName = "forumadmin",
                Email = "admin@example.com",
                NormalizedEmail = "admin@example.com",
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var hasher = new PasswordHasher<ApplicationUser>();
            var hashedPassword = hasher.HashPassword(user, "admin");

            user.PasswordHash = hashedPassword;


            var hasAdminRole = _context.Roles.Any(roles => roles.Name == "Admin");
            if (!hasAdminRole)
            {
                 roleStore.CreateAsync(new IdentityRole
                {
                    Name = "Admin",
                    NormalizedName = "admin"
                });
            }

            var hasSuperUser = _context.Users.Any(u => u.NormalizedUserName == user.UserName);
            if (!hasSuperUser)
            {
                 userStore.CreateAsync(user);
                 userStore.AddToRoleAsync(user,"Admin");
            }


             _context.SaveChangesAsync();


            return Task.CompletedTask;
        }

    }
}
