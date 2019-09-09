using LambdaForums.Models.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LambdaForums.Models.Search
{
    public class SearchResultsModel
    {
        public IEnumerable<PostListingModel> posts { get; set; }
        public string searchQuery { get; set; }
        public bool EmptySearchResults { get; set; }
    }
}
