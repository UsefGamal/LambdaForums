    using LambdaForums.Data;
using LambdaForums.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaForums.Service
{
    public class ApplicationUserService : IApplicationUser
    {
        private readonly ApplicationDbContext _context;
        public ApplicationUserService(ApplicationDbContext context)
        {
            _context = context;
        }
        public IEnumerable<ApplicationUser> GetAll()
        {
            return _context.ApplicationUsers;
        }

        public ApplicationUser GetById(string id)
        {
            return GetAll().FirstOrDefault(
                user => user.Id == id);
        }

        public async Task IncrementUserRating(string id, Type type)
        {
            var user = GetById(id);
            user.Rating = CalculateUserRating(type, user.Rating, '+');
            await _context.SaveChangesAsync();
        }
        public async Task DecrementUserRating(string id, Type type)
        {
            var user = GetById(id);
            user.Rating = CalculateUserRating(type, user.Rating, '-');
            await _context.SaveChangesAsync();
        }
        private int CalculateUserRating(Type type, int userRating, char c)
        {
            var value = 0;
            if (type == typeof(Post))
                value = 1;
            else if (type == typeof(PostReply))
                value = 3;
            if (c == '+')
                return userRating + value;
            else
                return userRating - value;
        }
        public async Task Deactivate(ApplicationUser user)
        {
            user.IsActive = false;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            _context.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task Activate(ApplicationUser user)
        {
            user.IsActive = true;
            user.LockoutEnd = null;
            _context.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task SetProfileImage(string id, string path)
        {
            var user = GetById(id);

            user.ProfileImageUrl = path;
            _context.Update(user);
            await _context.SaveChangesAsync();
        }


    }
}
