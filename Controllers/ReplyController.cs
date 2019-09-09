using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LambdaForums.Data;
using LambdaForums.Data.Models;
using LambdaForums.Models.Reply;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LambdaForums.Controllers
{
    [Authorize]
    public class ReplyController : Controller
    {
        private readonly IForum _forumService;
        private readonly IPost _postService;
        private readonly IApplicationUser _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _environment;


        public ReplyController(IForum forumService,
            IPost postService,
            IApplicationUser userService,
            UserManager<ApplicationUser> userManager,
              IHostingEnvironment IHostingEnvironment)
        {
            _forumService = forumService;
            _postService = postService;
            _userService = userService;
            _userManager = userManager;
            _environment = IHostingEnvironment;
        }
        public async Task<IActionResult> Create(int id)
        {
            var post = _postService.GetById(id);
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var model = new PostReplyModel
            {
                PostContent = post.Content,
                PostTitle = post.Title,
                PostId = post.Id,

                //AuthorName= user.UserName,
                AuthorId = user.Id,
                AuthorName = User.Identity.Name,
                AuthorImageUrl = user.ProfileImageUrl,
                AutorRating = user.Rating,
                IsAuthorAdmin = User.IsInRole("Admin"),

                Created = DateTime.Now,

                ForumId = post.Forum.Id,
                ForumName = post.Forum.Title,
                ForumImageUrl = post.Forum.ImageUrl
            };
            if (post.ImageUrl != "")
            {
                model.PostImageURL = post.ImageUrl;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddReply(PostReplyModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            //var post = _postService.GetById(model.PostId);
            //var imageUri = "";
            //if (model.ReplyImageUpload != null && model.ReplyImageUpload.Length != 0)
            //{
            //    //var blockBlob = UploadForumImage(model.ImageUpload);
            //    //imageUri = blockBlob.Uri.AbsoluteUri;
            //    string pathRoot = _environment.WebRootPath;
            //    string path_to_Images = pathRoot + "\\Images\\replies\\" + model.ReplyImageUpload.FileName;

            //    using (var stream = new FileStream(path_to_Images, FileMode.Create))
            //    {
            //        await model.ReplyImageUpload.CopyToAsync(stream);
            //    }

            //    imageUri = "/Images/replies/" + model.ReplyImageUpload.FileName;
            //}
            //PostReply reply = new PostReply
            //{
            //    Post = post,
            //    Content = model.ReplyContent,
            //    Created = DateTime.Now,
            //    User = user,
            //    ImageUrl = imageUri
            //};
            PostReply reply = await BuildReplyModel(model, user);
            await _postService.AddReply(reply);
            await _userService.IncrementUserRating(userId, typeof(PostReply));

            return RedirectToAction("Index", "Post", new { id = model.PostId });
        }
        public IActionResult Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var reply = _postService.GetReplyById(id);
            var post = _postService.GetById(reply.Post.Id);
            var forum = _forumService.GetById(post.Forum.Id);

            var model = new PostReplyModel
            {
                ForumName = forum.Title,
                ForumId = forum.Id,
                ForumImageUrl = forum.ImageUrl,

                PostContent = post.Content,
                PostTitle = post.Title,
                PostId = post.Id,
                PostImageURL = post.ImageUrl,

                AuthorId = userId,
                AuthorName = User.Identity.Name,

                ReplyId = reply.Id,
                ReplyContent = reply.Content,
                ReplyImageUrl = reply.ImageUrl,

            };

            return View(model);
        }
        public async Task<IActionResult> EditReply(PostReplyModel model)
        {

            var imageUri = model.ReplyImageUrl;
            if (model.ReplyImageUpload != null && model.ReplyImageUpload.Length != 0)
            {
                //var blockBlob = UploadForumImage(model.ImageUpload);
                //imageUri = blockBlob.Uri.AbsoluteUri;
                string pathRoot = _environment.WebRootPath;
                string path_to_Images = pathRoot + "\\Images\\replies\\" + model.ReplyImageUpload.FileName;

                using (var stream = new FileStream(path_to_Images, FileMode.Create))
                {
                    await model.ReplyImageUpload.CopyToAsync(stream);
                }

                imageUri = "/Images/replies/" + model.ReplyImageUpload.FileName;
            }
            model.ReplyImageUrl = imageUri;
            await _postService.EditReply(model.ReplyId, model.ReplyContent, model.ReplyImageUrl);

            var id = model.PostId;
            return RedirectToAction("Index", "Post", new { id });
        }
        public IActionResult Delete(int id)
        {

            var reply = _postService.GetReplyById(id);
            var user = _userService.GetById(reply.User.Id);
            var post = _postService.GetById(reply.Post.Id);
            var forum = _forumService.GetById(post.Forum.Id);


            var model = new PostReplyDeleteModel
            {
                ForumId = forum.Id,
                ForumName = forum.Title,
                ForumImageUrl = forum.ImageUrl,

                PostId = post.Id,
                PostTitle = post.Title,

                AuthorId = user.Id,
                AuthorEmail = user.Email,
                AutorRating = user.Rating,
                AuthorName = user.UserName, 
                MemberSince=user.MemberSince,

                AuthorImageUrl = user.ProfileImageUrl,
                IsAuthorAdmin = IsAuthorAdmin(reply.User),

                ReplyId = reply.Id,
                ReplyContent = reply.Content,
                ReplyImageUrl = reply.ImageUrl,
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var reply = _postService.GetReplyById(id);
            var post = _postService.GetById(reply.Post.Id);

            if (System.IO.File.Exists(_environment.WebRootPath + reply.ImageUrl))
            {
                System.IO.File.Delete(_environment.WebRootPath + reply.ImageUrl);
            }

            await _userService.DecrementUserRating(reply.User.Id, typeof(PostReply));
            await _postService.DeleteReply(reply.Id);

            return RedirectToAction("index", "Post", new {id = post.Id });
        }
        private async Task<PostReply> BuildReplyModel(PostReplyModel model, ApplicationUser user)
        {
            var post = _postService.GetById(model.PostId);
            var imageUri = "";
            if (model.ReplyImageUpload != null && model.ReplyImageUpload.Length != 0)
            {
                //var blockBlob = UploadForumImage(model.ImageUpload);
                //imageUri = blockBlob.Uri.AbsoluteUri;
                string pathRoot = _environment.WebRootPath;
                string path_to_Images = pathRoot + "\\Images\\replies\\" + model.ReplyImageUpload.FileName;

                using (var stream = new FileStream(path_to_Images, FileMode.Create))
                {
                    await model.ReplyImageUpload.CopyToAsync(stream);
                }

                imageUri = "/Images/replies/" + model.ReplyImageUpload.FileName;
            }

            return  new PostReply
            {
                Post = post,
                Content = model.ReplyContent,
                Created = DateTime.Now,
                User = user,
                ImageUrl = imageUri
            };
        }
        private bool IsAuthorAdmin(ApplicationUser user)
        {
            return _userManager.GetRolesAsync(user)
                .Result.Contains("Admin");
        }
    }
}