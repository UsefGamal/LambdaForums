using System;

namespace LambdaForums.Data.Models
{
    public class PostReply
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public string ImageUrl { get; set; }


        public virtual ApplicationUser User { get; set; }
        public virtual Post Post { get; set; }

    }
}
