using EtherApp.Data.Models;
using System.Collections.Generic;

namespace EtherApp.Shared.ViewModels.Search
{
    public class SearchResultsVM
    {
        public string Query { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new List<User>();
        public List<Post> Posts { get; set; } = new List<Post>();
    }
}