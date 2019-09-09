using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LambdaForums.Models.Reply
{
    public class PostReplyModel
    {
        public int ReplyId { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorImageUrl { get; set; }
        public int AutorRating { get; set; }
        public bool IsAuthorAdmin { get; set; }
        public bool IsAuthorActive { get; set; }


        public DateTime Created { get; set; }
        public string ReplyContent { get; set; }
        public string ReplyImageUrl { get; set; }
        public IFormFile ReplyImageUpload { get; set; }

        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public string PostContent { get; set; }
        public string PostImageURL { get; set; }
        public string ForumName { get; set; }
        public string ForumImageUrl { get; set; }
        public int ForumId { get; set; }
    }
}
