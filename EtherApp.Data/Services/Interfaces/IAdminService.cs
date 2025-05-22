using EtherApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<Post>> GetReportedPostsAsync();
        Task<bool> ApprovePostAsync(int postId);
        Task<bool> DeletePostAsync(int postId);


    }
}
