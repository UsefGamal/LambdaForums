using LambdaForums.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LambdaForums.Data
{
    public interface IPost
    {
        Post GetById(int id);
        IEnumerable<Post> GetAll();
        IEnumerable<Post> GetFilteredPosts(Forum forum, string searchQuery);
        IEnumerable<Post> GetFilteredPosts( string searchQuery);

        IEnumerable<Post> GetPostsByForum(int id);
        IEnumerable<Post> GetPostsByUser(string id);
        IEnumerable<Post> GetLatestPost(int numberOfPosts);
        IEnumerable<ApplicationUser> GetAllUsers(IEnumerable<Post> posts);

        Task AddPost(Post post);
        Task DeletePost(int id);
        Task EditPost(int id, string newContent, string newTitle, string newImageUrl);

        Task AddReply(PostReply reply);
        PostReply GetReplyById(int id);
        Task EditReply(int id, string newContent, string newImageUrl);
        Task DeleteReply(int id);
    }
}
