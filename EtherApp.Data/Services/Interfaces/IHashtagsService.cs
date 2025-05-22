using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services.Interfaces
{
    public interface IHashtagsService
    {
        Task ProcessHashtagsForNewPostAsync(string content, int loggedInUserId);
        Task ProcessHashtagsForRemovedPostAsync(string content, int loggedInUserId);
    }
}
