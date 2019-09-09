using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LambdaForums.Models.Post
{
    public class NewPostModel
    {
        public int PostId { get; set; }
        public string ForumName { get; set; }
        public int ForumId { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorImageUrl { get; set; }
        public int AutorRating { get; set; }
        public string ForumImageUrl { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string PostImageUrl { get; set; }
        public IFormFile PostImageUpload { get; set; }
        public bool IsAuthorAdmin { get; set; }
        public string Email { get; set; }
        public DateTime MemberSince { get; set; }

    }
}
