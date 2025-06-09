using EtherApp.Data.Models;
using System.Collections.Generic;

namespace EtherApp.ViewModels.Discovery
{
    public class UserDiscoveryVM
    {
        // Users to display
        public List<UserDiscoveryItemVM> DiscoveredUsers { get; set; } = new List<UserDiscoveryItemVM>();

        // Filter properties
        public List<Interest> AvailableInterests { get; set; } = new List<Interest>();
        public List<int> SelectedInterests { get; set; } = new List<int>();
        public bool FilterByMyInterests { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class UserDiscoveryItemVM
    {
        public User User { get; set; }
        public int MatchPercentage { get; set; }
        public List<Interest> SharedInterests { get; set; } = new List<Interest>();
        public bool IsFriend { get; set; }
        public bool HasPendingFriendRequest { get; set; }
        public string MostRecentPost { get; set; }
        public int PostCount { get; set; }
    }
}