using LambdaForums.Data;
using LambdaForums.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LambdaForums.Service
{
    public class ForumService : IForum
    {
        private readonly ApplicationDbContext _context;
        private readonly IPost _postService;

        public ForumService(ApplicationDbContext context, IPost postService)
        {
            _context = context;
            _postService = postService;
        }

        public async Task Create(Data.Models.Forum forum)
        {
            _context.Add(forum);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var forum = GetById(id);
            _context.Remove(forum);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<ApplicationUser> GetActiveUsers(int forumId)
        {
            var posts = GetById(forumId).Posts;

            if (posts == null || !posts.Any())
            {
                return new List<ApplicationUser>();
            }

            return _postService.GetAllUsers(posts);
        }

        public IEnumerable<Data.Models.Forum> GetAll()
        {
            return _context.Forums
                .Include(forum => forum.Posts);
        }

        public Data.Models.Forum GetById(int id)
        {
            var forum = _context.Forums
                .Where(f => f.Id == id)
                .Include(f => f.Posts)
                .ThenInclude(f => f.User)
                .Include(f => f.Posts)
                .ThenInclude(f => f.Replies)
                .ThenInclude(f => f.User)
                .Include(f => f.Posts)
                .ThenInclude(p => p.Forum)
                .FirstOrDefault();

            if (forum.Posts == null)
            {
                forum.Posts = new List<Post>();
            }

            return forum;
        }

        public Post GetLatestPost(int forumId)
        {
            var posts = GetById(forumId).Posts;

            if (posts != null)
            {
                return GetById(forumId).Posts
                    .OrderByDescending(post => post.Created)
                    .FirstOrDefault();
            }

            return new Post();
        }

        public bool HasRecentPost(int id)
        {
            const int hoursAgo = 12;
            var window = DateTime.Now.AddHours(-hoursAgo);
            return GetById(id).Posts.Any(post => post.Created >= window);
        }

        public async Task Add(Data.Models.Forum forum)
        {
            _context.Add(forum);
            await _context.SaveChangesAsync();
        }

        public async Task SetForumImage(int id, Uri uri)
        {
            var forum = GetById(id);
            forum.ImageUrl = uri.AbsoluteUri;
            _context.Update(forum);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<Post> GetFilteredPosts(string searchQuery)
        {
            return _postService.GetFilteredPosts(searchQuery);
        }

        public IEnumerable<Post> GetFilteredPosts(int forumId, string searchQuery)
        {
            if (forumId == 0) return _postService.GetFilteredPosts(searchQuery);

            var forum = GetById(forumId);

            return string.IsNullOrEmpty(searchQuery)
                ? forum.Posts
                : forum.Posts.Where(post
                    => post.Title.Contains(searchQuery) || post.Content.Contains(searchQuery));
        }

        public async Task UpdateForum(int id, string newTitle, string newDescripition,string newImageUrl)
        {
            var forum = GetById(id);
            forum.Title = newTitle;
            forum.Descripition = newDescripition;
            forum.ImageUrl = newImageUrl;
            _context.Update(forum);
            await _context.SaveChangesAsync();
        }

     


    }
}