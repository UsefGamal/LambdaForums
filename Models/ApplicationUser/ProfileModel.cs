using LambdaForums.Models.Post;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace LambdaForums.Models.ApplicationUser
{
    public class ProfileModel
    {
        public string id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string UserRating { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public DateTime MemberSince { get; set; }
        public IFormFile ImageUpload { get; set; }
        public IEnumerable<PostListingModel> UserPosts { get; set; }
    }
}
