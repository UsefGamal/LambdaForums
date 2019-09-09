using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LambdaForums.Data;
using LambdaForums.Data.Models;
using LambdaForums.Models.Post;
using LambdaForums.Models.Reply;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LambdaForums.Controllers
{
    public class PostController : Controller
    {
        private readonly IPost _postService;
        private readonly IForum _forumService;
        //private readonly IPostFormatter _postFormatter;
        private readonly IApplicationUser _userService;
        private static UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _environment;

        public PostController(IPost postService,
                              IForum forumService,
                              IApplicationUser userService, 
                              UserManager<ApplicationUser> userManager,
                              IHostingEnvironment IHostingEnvironment
                              /*, IPostFormatter postFormatter*/)
                              {
                                    _postService = postService;
                                    _forumService = forumService;
                                    _userManager = userManager;
                                    _userService = userService;
                                    _environment = IHostingEnvironment;
                                    //_postFormatter = postFormatter;
                              }
        public IActionResult Index(int id)
        {
            var post = _postService.GetById(id);
            if (post.Replies.Any())
            {
                foreach (var reply in post.Replies)
                {
                    if (reply.Content != null)
                    {
                        foreach (Match item in Regex.Matches(reply.Content, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                        {
                            string LinkInHtml = ConvertTextUrlToLink(item.Value);
                            reply.Content.Replace(item.Value, LinkInHtml);
                        }
                    }
                   
                }
            }
            
            var replies = BuildPostReplies(post.Replies);
            var model = new PostIndexModel
            {
                Id = post.Id,
                Title = post.Title,
                AuthorId = post.User.Id,
                AuthorImageUrl = post.User.ProfileImageUrl,
                AuthorName = post.User.UserName,
                AutorRating = post.User.Rating,
                Created = post.Created,
                PostContent = post.Content,
                Replies = replies,
                IsAuthorActive=post.User.IsActive,
                IsAuthorAdmin= IsAuthorAdmin(post.User),
                ForumId=post.Forum.Id,
                ForumName= post.Forum.Title
               
            };

            if (post.ImageUrl !="" && post.ImageUrl!= null)
            {
                model.PostImageUrl = post.ImageUrl;
            }
            return View(model);
        }

        [Authorize]
        public IActionResult Create(int id)
        {// id is For Forum.Id
            var userId = _userManager.GetUserId(User);

            var forum = _forumService.GetById(id);
            var model = new NewPostModel
            {
                ForumName = forum.Title,
                ForumId = forum.Id,
                ForumImageUrl = forum.ImageUrl,
                AuthorName = User.Identity.Name,
                AuthorId=userId

            };
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPost(NewPostModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = _userManager.FindByIdAsync(userId).Result;
            var forum = _forumService.GetById(model.ForumId);
            var imageUri = "";
            if (model.PostImageUpload != null && model.PostImageUpload.Length != 0)
            {
                //var blockBlob = UploadForumImage(model.ImageUpload);
                //imageUri = blockBlob.Uri.AbsoluteUri;
                string pathRoot = _environment.WebRootPath;
                string path_to_Images = pathRoot + "\\Images\\posts\\" + model.PostImageUpload.FileName;

                using (var stream = new FileStream(path_to_Images, FileMode.Create))
                {
                    await model.PostImageUpload.CopyToAsync(stream);
                }

                imageUri = "/Images/posts/" + model.PostImageUpload.FileName;
            }

            Post post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                Created = DateTime.Now,
                User = user,
                Forum = forum,
                ImageUrl = imageUri
            };

            await _postService.AddPost(post);// Block The Current Thread Untill the task is Complete

            await _userService.IncrementUserRating(userId, typeof(Post));
            
            return RedirectToAction("Index","Post",new { post.Id });
        }
        public IActionResult Edit(int id)
        {
            var post = _postService.GetById(id);
            var userId = _userManager.GetUserId(User);
            var forum = _forumService.GetById(post.Forum.Id);
            var model = new NewPostModel
            {
                ForumName = forum.Title,
                ForumId = forum.Id,
                ForumImageUrl = forum.ImageUrl,
                AuthorName = User.Identity.Name,
                AuthorId = userId,
                PostId =post.Id,
                Title = post.Title,
                Content = post.Content,
                PostImageUrl= post.ImageUrl
            };

            return View(model);
        }

        public async Task<IActionResult> EditPost(NewPostModel model)
        {
            var imageUri = model.PostImageUrl;
            if (model.PostImageUpload != null && model.PostImageUpload.Length != 0)
            {
                //var blockBlob = UploadForumImage(model.ImageUpload);
                //imageUri = blockBlob.Uri.AbsoluteUri;
                string pathRoot = _environment.WebRootPath;
                string path_to_Images = pathRoot + "\\Images\\posts\\" + model.PostImageUpload.FileName;

                using (var stream = new FileStream(path_to_Images, FileMode.Create))
                {
                    await model.PostImageUpload.CopyToAsync(stream);
                }

                imageUri = "/Images/posts/" + model.PostImageUpload.FileName;
            }

            model.PostImageUrl = imageUri;
            await _postService.EditPost(model.PostId,model.Content,model.Title,model.PostImageUrl);

            return RedirectToAction("Index", "Post", new {id= model.PostId });
        }

        public IActionResult Delete(int id)
        {
            var post = _postService.GetById(id);
            var forum = _forumService.GetById(post.Forum.Id);

            var model = new NewPostModel
            {
                ForumId = forum.Id,
                ForumName = forum.Title,
                ForumImageUrl = forum.ImageUrl,

                AuthorId = post.User.Id,
                Email = post.User.Email,
                AutorRating = post.User.Rating,
                AuthorName = post.User.UserName,
                MemberSince = post.User.MemberSince,
                IsAuthorAdmin = IsAuthorAdmin(post.User),
                AuthorImageUrl = post.User.ProfileImageUrl,

                PostId = post.Id,
                Title = post.Title,
                Content = post.Content,
                PostImageUrl = post.ImageUrl
            };

            return View(model);
        }
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var post = _postService.GetById(id);
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

            return RedirectToAction("Index", "Forum", new { id = post.Forum.Id });
        }
        private bool IsAuthorAdmin(ApplicationUser user)
        {
            return _userManager.GetRolesAsync(user)
                .Result.Contains("Admin");
        }

        private IEnumerable<PostReplyModel> BuildPostReplies(IEnumerable<PostReply> replies)
        {
            return replies.Select( reply=>new PostReplyModel
            {
                ReplyId=reply.Id,
                AuthorId = reply.User.Id,
                AuthorImageUrl = reply.User.ProfileImageUrl,
                AuthorName = reply.User.UserName,
                AutorRating = reply.User.Rating,
                Created = reply.Created,
                ReplyContent=reply.Content,
                ReplyImageUrl= reply.ImageUrl,
                IsAuthorActive=reply.User.IsActive,
                IsAuthorAdmin=IsAuthorAdmin(reply.User)
                
            });
        }
        private string ConvertTextUrlToLink(string url)
        {
            string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[_.a-z0-9-]+\.
       [a-z0-9\/_:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(url, "<a href="+url+" title =\"Click here to open in a new window or tab\"  target =\"_blank\">" + url+"</a>");
        }
    }
}