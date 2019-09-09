using LambdaForums.Data;
using LambdaForums.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambdaForums.Service
{
    public class PostService : IPost
    {
        private readonly ApplicationDbContext _context;
        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddPost(Post post)
        {
            _context.Add(post);
            await _context.SaveChangesAsync();
        }
        public async Task AddReply(PostReply reply)
        {
            _context.Add(reply);
            await _context.SaveChangesAsync();
        }
        public async Task DeletePost(int id)
        {
            var post = GetById(id);
            _context.Remove(post);
            await _context.SaveChangesAsync();
        }
        public async Task EditPost(int id, string newContent, string newTitle, string newImageUrl)
        {
            var post = _context.Posts.Find(id);
            post.Content = newContent;
            post.Title = newTitle;
            post.ImageUrl = newImageUrl;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }
        public async Task EditReply(int id, string newContent,  string newImageUrl)
        {
            var reply = _context.PostReplies.Find(id);
            reply.Content = newContent;
            reply.ImageUrl = newImageUrl;

            _context.PostReplies.Update(reply);
            await _context.SaveChangesAsync();
        }
        public IEnumerable<Post> GetAll()
        {
            return _context.Posts
                .Include(post => post.User)
                .Include(post => post.Replies)
                        .ThenInclude(reply => reply.User)
                .Include(post => post.Forum);
        }
        public Post GetById(int id)
        {
            return _context.Posts.Where(post => post.Id == id)
                .Include(post => post.User)
                .Include(post => post.Replies)
                        .ThenInclude(reply => reply.User)
                .Include(post => post.Forum)
                .FirstOrDefault();
        }
        public IEnumerable<Post> GetFilteredPosts(Forum forum, string searchQuery)
        {
            if (String.IsNullOrEmpty(searchQuery))
                return forum.Posts;
            else
            {
                var normalized = searchQuery.ToLower();
                return
                    forum.Posts.Where(post
                    => post.Title.ToLower().Contains(normalized)
                    || post.Content.ToLower().Contains(normalized));
            }
        }
        public IEnumerable<Post> GetFilteredPosts(string searchQuery)
        {
            var normalized = searchQuery.ToLower();
            return GetAll().Where(post
                 => post.Title.ToLower().Contains(normalized)
                 || post.Content.ToLower().Contains(normalized));
        }
        public IEnumerable<Post> GetLatestPost(int n)
        {
            return GetAll().OrderByDescending(post => post.Created).Take(n);
        }
        public IEnumerable<Post> GetPostsByForum(int id)
        {
            return _context.Forums.Where(p => p.Id == id)
                .First().Posts;
        }
        public IEnumerable<Post> GetPostsByUser(string id)
        {
            return GetAll().Where(post => post.User.Id == id);
        }

        public IEnumerable<ApplicationUser> GetAllUsers(IEnumerable<Post> posts)
        {
            var users = new List<ApplicationUser>();

            foreach (var post in posts)
            {
                users.Add(post.User);

                if (!post.Replies.Any()) continue;

                users.AddRange(post.Replies.Select(reply => reply.User));
            }

            return users.Distinct();
        }

        public PostReply GetReplyById(int id)
        {
            return _context.PostReplies.Where(postReply => postReply.Id == id)
                 .Include(post => post.User)
                 .Include(post => post.Post)
                         .ThenInclude(reply => reply.User)
                 .Include(post => post.Post.Forum)
                 .FirstOrDefault();
        }

        public async Task DeleteReply(int id)
        {
            var reply = GetReplyById(id);
            _context.Remove(reply);
            await _context.SaveChangesAsync();
        }
    }
}

