using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LambdaForums.Data;
using LambdaForums.Data.Models;
using LambdaForums.Models.ApplicationUser;
using LambdaForums.Models.Forum;
using LambdaForums.Models.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LambdaForums.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUser _userService;
        private readonly IUpload _uploadService;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;
        private readonly IPost _postService;

        public ProfileController(
            IPost postService,
            UserManager<ApplicationUser> userManager,
            IApplicationUser userService,
            IUpload uploadService,
            IConfiguration configuration,
            IHostingEnvironment IHostingEnvironment)
        {
            _postService = postService;
            _userManager = userManager;
            _userService = userService;
            _uploadService = uploadService;
            _configuration = configuration;
            _environment = IHostingEnvironment;
        }
        [HttpGet]
        public IActionResult Detail(string id)
        {
            var user = _userService.GetById(id);
            var userRoles = _userManager.GetRolesAsync(user).Result;
            var userPosts = _postService.GetPostsByUser(id);

            var posts = userPosts.Select(post => new PostListingModel
            {
                Id = post.Id,
                Title = post.Title,
                AuthorName = post.User.UserName,
                AuthorId = post.User.Id,
                AuthorRating = post.User.Rating,
                DatePosted = post.Created,
                RepliesCount = post.Replies.Count(),
                Forum = GetForumListingForPost(post)
            }).OrderByDescending(x => x.DatePosted);

            var model = new ProfileModel()
            {
                id = user.Id,
                UserName = user.UserName,
                UserRating = user.Rating.ToString(),
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl,
                MemberSince = user.MemberSince,
                UserPosts = posts,
                IsActive=user.IsActive,
                IsAdmin = userRoles.Contains("Admin")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            var userId = _userManager.GetUserId(User);

            if (file == null || file.Length == 0)
            {
                return RedirectToAction("Detail", "Profile", new { id = userId });
            }
            string pathRoot = _environment.WebRootPath;
            string path_to_Images = pathRoot + "\\Images\\users\\" + file.FileName;

            using (var stream = new FileStream(path_to_Images, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            //set users profile Image to the URI 
            await _userService.SetProfileImage(userId, "/Images/users/" + file.FileName);
            // REdirect To users Profile page 
            return RedirectToAction("Detail", "Profile", new { id = userId });

            ////Connect To Azure Storage Account Container
            //var connectionString = _configuration.GetConnectionString("AzureStorageAccount");
            ////Get Blob Coontainer
            //var container = _uploadService.GetBlobContainer(connectionString,"profile-images");
            ////Parse The Content Disposition response header
            //var contentDisposition = ContentDispositionHeaderValue.Parse(file.ContentDisposition);
            ////Grab The FileName
            //var fileName = contentDisposition.FileName.Trim('"');
            ////Get a refrence to a Block BLob 
            //var blockBlob = container.GetBlockBlobReference(fileName);
            ////on The block Blob upload ur file <--file uploaded to the cloud -->
            //await blockBlob.UploadFromStreamAsync(file.OpenReadStream());

        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var profiles = _userService.GetAll()
                .OrderByDescending(user => user.Rating)
                .Select(user => new ProfileModel
                {
                    id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    ProfileImageUrl = user.ProfileImageUrl,
                    UserRating = user.Rating.ToString(),
                    MemberSince = user.MemberSince,
                    IsActive=user.IsActive,
                    IsAdmin = IsUserAdmin(user)
                    
                });
            var model = new ProfileListModel
            {
                Profiles = profiles
            };

            return View(model);
        }
        public IActionResult Deactivate(string id)
        {
            var user = _userService.GetById(id);
            _userService.Deactivate(user);
            return RedirectToAction("Index", "Profile");
        }
        public IActionResult Activate(string id)
        {
            var user = _userService.GetById(id);
            _userService.Activate(user);
            return RedirectToAction("Index", "Profile");
        }
        private ForumListingModel GetForumListingForPost(Post post)
        {
            var forum = post.Forum;
            return new ForumListingModel
            {
                Id = forum.Id,
                Name = forum.Title,
                ImageUrl = forum.ImageUrl

            };
        }
        private bool IsUserAdmin(ApplicationUser user)
        {
            return _userManager.GetRolesAsync(user)
                .Result.Contains("Admin");
        }
    }
}