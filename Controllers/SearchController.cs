using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LambdaForums.Data;
using LambdaForums.Data.Models;
using LambdaForums.Models.Forum;
using LambdaForums.Models.Post;
using LambdaForums.Models.Search;
using Microsoft.AspNetCore.Mvc;

namespace LambdaForums.Controllers
{
    public class SearchController : Controller
    {
        private readonly IPost _postService;
        public SearchController(IPost postService)
        {
            _postService = postService;
        }
        public IActionResult Results(string searchQuery)
        {
            var posts = _postService.GetFilteredPosts(searchQuery);

            var areNoResults = (!string.IsNullOrEmpty(searchQuery) && !posts.Any());    

            var postListing = posts.Select(post => new PostListingModel
            {
                Id = post.Id,
                AuthorId = post.User.Id,
                AuthorRating = post.User.Rating,
                AuthorName = post.User.UserName,
                Title = post.Title,
                DatePosted = post.Created,
                RepliesCount = post.Replies.Count(),
                Forum = BuildForumListing(post)

            });
            var model = new SearchResultsModel {
                posts = postListing,
                searchQuery=searchQuery,
                EmptySearchResults= areNoResults
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult Search(string searchQuery)
        {
            return RedirectToAction("Results", new { searchQuery });
        }
        private ForumListingModel BuildForumListing(Post post)
        {
            var forum = post.Forum;

            return new ForumListingModel
            {
                Id = forum.Id,
                Name = forum.Title,
                Description = forum.Descripition,
                ImageUrl = forum.ImageUrl
            };
        }


    }
}