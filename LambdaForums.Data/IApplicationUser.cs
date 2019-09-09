using LambdaForums.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LambdaForums.Data
{
   public interface IApplicationUser
    {
        ApplicationUser GetById(string id);
        IEnumerable<ApplicationUser> GetAll();
        Task SetProfileImage(string id, string path);
        Task IncrementUserRating(string id ,Type type);
        Task DecrementUserRating(string id ,Type type);
        Task Deactivate(ApplicationUser user);
        Task Activate(ApplicationUser user);


    }
}
