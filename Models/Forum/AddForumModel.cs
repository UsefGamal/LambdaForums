
using Microsoft.AspNetCore.Http;

namespace LambdaForums.Models.Forum
{
    public class AddForumModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public IFormFile ImageUpload { get; set; }
    }
}
