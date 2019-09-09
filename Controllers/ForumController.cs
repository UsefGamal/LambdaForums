using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LambdaForums.Data;
using LambdaForums.Data.Models;
using LambdaForums.Models.Forum;
using LambdaForums.Models.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;

namespace LambdaForums.Controllers
{
    public class ForumController : Controller
    {
        private readonly IForum _forumService;
        private readonly IPost _postService;
        private readonly IApplicationUser _userService;
        private readonly IUpload _uploadService;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        public ForumController(
            IForum forumService,
            IPost postService,
            IApplicationUser userService,
            IUpload uploadService,
            IConfiguration configuration,
            IHostingEnvironment IHostingEnvironment)
        {
            _forumService = forumService;
            _postService = postService;
            _uploadService = uploadService;
            _userService = userService;
            _configuration = configuration;
            _environment = IHostingEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<ForumListingModel> forums = _forumService
                .GetAll().Select(forum=>new ForumListingModel {
                    Id=forum.Id,
                    Name= forum.Title,
                    Description= forum.Descripition,
                    ImageUrl=forum.ImageUrl,
                    NumberOfPosts=forum.Posts?.Count() ?? 0,
                    NumberOfUsers=_forumService.GetActiveUsers(forum.Id).Count(),
                    HasRecentPost=_forumService.HasRecentPost(forum.Id)
                });

            var model = new ForumIndexModel
            {
                ForumList = forums.OrderBy(f => f.Name)
            };
            return View(model);
        }
        public IActionResult Topic(int id,string searchQuery)
        {
            var forum = _forumService.GetById(id);
            var posts = new List<Post>();
               posts = _postService.GetFilteredPosts(forum, searchQuery).ToList();           
            var postListings = posts.Select(post => new PostListingModel
            {
                Id= post.Id,
                AuthorId=post.User.Id,
                AuthorRating=post.User.Rating,
                AuthorName=post.User.UserName,
                Title=post.Title,
                DatePosted = post.Created,
                RepliesCount=post.Replies.Count(),
                Forum = BuildForumListing(post)
            });

            var model = new ForumTopicModel
            {
                Posts = postListings,
                Forum = BuildForumListing(forum)
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult Search(int id, string searchQuery) {
                

            return RedirectToAction("Topic",new {id,searchQuery });
        }
        [Authorize(Roles="Admin")]
        public IActionResult Create()
        {
            var model = new AddForumModel();
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddForum(AddForumModel model)
        {
            var imageUri = "/images/users/default.png";

            if (model.ImageUpload!=null && model.ImageUpload.Length != 0)
            {
                //var blockBlob = UploadForumImage(model.ImageUpload);
                //imageUri = blockBlob.Uri.AbsoluteUri;


                string pathRoot = _environment.WebRootPath;
                string path_to_Images = pathRoot + "\\Images\\forum\\" + model.ImageUpload.FileName;

                using (var stream = new FileStream(path_to_Images, FileMode.Create))
                {
                    await model.ImageUpload.CopyToAsync(stream);
                }

                imageUri = "/Images/forum/" + model.ImageUpload.FileName; 
            }
            var forum = new Forum
            {
                Title= model.Title,
                Descripition=model.Description,
                Created= DateTime.Now,
                ImageUrl=imageUri
            };
            await _forumService.Create(forum);
            return RedirectToAction("Index", "Forum");
        }
        public IActionResult Edit(int id )
        {
            var forum = _forumService.GetById(id);
            var model = new AddForumModel
                {
                    Id=forum.Id,
                    Title=forum.Title,
                    Description=forum.Descripition,
                    ImageUrl=forum.ImageUrl
                };
            return View(model);
        }
        public async Task<IActionResult> EditForum(AddForumModel model)
        {
            var imageUri = model.ImageUrl;
            if (model.ImageUrl=="")
                imageUri = "/images/users/default.png";

            if (model.ImageUpload != null && model.ImageUpload.Length != 0)
            {
                //var blockBlob = UploadForumImage(model.ImageUpload);
                //imageUri = blockBlob.Uri.AbsoluteUri;


                string pathRoot = _environment.WebRootPath;
                string path_to_Images = pathRoot + "\\Images\\forum\\" + model.ImageUpload.FileName;

                using (var stream = new FileStream(path_to_Images, FileMode.Create))
                {
                    await model.ImageUpload.CopyToAsync(stream);
                }

                imageUri = "/Images/forum/" + model.ImageUpload.FileName;
            }
            model.ImageUrl = imageUri;
           await  _forumService.UpdateForum(model.Id, model.Title, model.Description, model.ImageUrl);
          return  RedirectToAction("index");
        }

        public IActionResult Delete(int id)
        {
            var forum = _forumService.GetById(id);
            var posts = _postService.GetPostsByForum(id);

                var postListings = posts.Select(post => new PostListingModel
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
            var model = new ForumTopicModel
            {
               Forum = BuildForumListing(forum),
               Posts = postListings

            };
            return View(model);

        }
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var forum = _forumService.GetById(id);
            var posts = _postService.GetPostsByForum(id);
            if (posts.Any())
            {
                foreach (var post in posts.ToList())
                {
                    if (post.Replies.Any())
                    {
                        foreach (var reply in post.Replies.ToList())
                        {
                            if (System.IO.File.Exists(_environment.WebRootPath + reply.ImageUrl))
                            {
                                System.IO.File.Delete(_environment.WebRootPath + reply.ImageUrl);
                            }
                            await _userService.DecrementUserRating(reply.User.Id, typeof(PostReply));
                            await _postService.DeleteReply(reply.Id);
                        }
                    }
                    if (System.IO.File.Exists(_environment.WebRootPath + post.ImageUrl))
                    {
                        System.IO.File.Delete(_environment.WebRootPath + post.ImageUrl);
                    }
                    await _userService.DecrementUserRating(post.User.Id, typeof(Post));
                    await _postService.DeletePost(post.Id);
                }
            }
            if (System.IO.File.Exists(_environment.WebRootPath + forum.ImageUrl))
            {
                System.IO.File.Delete(_environment.WebRootPath + forum.ImageUrl);
            }
            await _forumService.Delete(forum.Id);

            return RedirectToAction("Index", "Forum");
        }

        private CloudBlockBlob UploadForumImage(IFormFile file)
        {
            var connectionString = _configuration.GetConnectionString("AzureStorageAccount");
            var container = _uploadService.GetBlobContainer(connectionString, "forum-images");
            var contentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);
            var fileName = contentDisposition.FileName.Trim('"');
            var blockBlob = container.GetBlockBlobReference(fileName);
            blockBlob.UploadFromStreamAsync(file.OpenReadStream()).Wait();
            return blockBlob;
        }
        
        private ForumListingModel BuildForumListing(Post post)
        {
            var forum = post.Forum;
            return BuildForumListing(forum);
        }
        private ForumListingModel BuildForumListing(Forum forum)
        {
            return new ForumListingModel
            {
                Id = forum.Id,
                Name = forum.Title,
                Description = forum.Descripition,
                ImageUrl = forum.ImageUrl,
                NumberOfPosts = forum.Posts?.Count() ?? 0,
                NumberOfUsers = _forumService.GetActiveUsers(forum.Id).Count(),
            };
        }
    }
}