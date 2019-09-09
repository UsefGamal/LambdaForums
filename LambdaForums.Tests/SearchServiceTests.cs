using LambdaForums.Data;
using LambdaForums.Data.Models;
using LambdaForums.Service;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;

namespace LambdaForums.Tests
{
    [TestFixture]
    public class Post_Service_Should
    {
        [TestCase("coffee",3)]
        [TestCase("TeA",1)]
        [TestCase("water",0)]
        public void Return_Filtered_Results_Corresponding_To_Query(string query,int expected)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            //Arrange
            using (var ctx = new ApplicationDbContext(options))
            {
                ctx.Forums.Add(new Forum
                {
                    Id =100
                });
                ctx.Posts.Add(new Post
                {
                    Forum = ctx.Forums.Find(100),
                    Id = 23523,
                    Title = "First Post",
                    Content = "Coffee"
                });
                ctx.Posts.Add(new Post
                {
                    Forum = ctx.Forums.Find(100),
                    Id = -2543,
                    Title = "Coffee",
                    Content = "Some Content"
                });
                ctx.Posts.Add(new Post
                {
                    Forum = ctx.Forums.Find(100),
                    Id = 25154,
                    Title = "Tea",
                    Content = "Coffee"
                });
                ctx.SaveChanges();
            }
            //Act
            using (var ctx = new ApplicationDbContext(options))
            {
                var postService = new PostService(ctx);
                var result = postService.GetFilteredPosts(query);
                var postCount = result.Count();
                //Assertion
                
                Assert.AreEqual(expected, postCount);
            }
        }
    }
}
