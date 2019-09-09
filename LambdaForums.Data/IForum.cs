using LambdaForums.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LambdaForums.Data
{
    public interface IForum
    {
        Forum GetById(int Id);
        IEnumerable<Forum> GetAll();


        Task Create(Forum forum);
        Task Delete(int forumId);
        Task UpdateForum(int forumId, string newTitle, string newDescription,string newImageUrl);
        IEnumerable<ApplicationUser> GetActiveUsers(int id);
        bool HasRecentPost(int id);
    }
}
