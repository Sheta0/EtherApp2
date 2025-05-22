using EtherApp.Data.Helpers;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services.Implementations
{
    public class HashtagService : IHashtagsService
    {
        private readonly AppDBContext _context;
        public HashtagService(AppDBContext context)
        {
            _context = context;
        }
        public async Task ProcessHashtagsForNewPostAsync(string content, int loggedInUserId)
        {
            // Find and store the hashtags
            var postHashtags = HashtagsHelper.GetHashtags(content);
            foreach (var hashtag in postHashtags)
            {
                var hashtagDb = await _context.Hashtags.FirstOrDefaultAsync(h => h.Name == hashtag);
                if (hashtagDb != null)
                {
                    hashtagDb.Count++;
                    hashtagDb.DateUpdate = DateTime.Now;

                    _context.Hashtags.Update(hashtagDb);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var newHashtag = new Hashtag()
                    {
                        Name = hashtag,
                        Count = 1,
                        DateUpdate = DateTime.Now,
                        DateCreate = DateTime.Now,
                    };

                    await _context.Hashtags.AddAsync(newHashtag);
                    await _context.SaveChangesAsync();
                    hashtagDb = newHashtag;

                    // Update the UserHashtag table
                    var userHashtag = new UserHashtag
                    {
                        UserId = loggedInUserId,
                        HashtagId = hashtagDb.Id
                    };

                    await _context.UserHashtags.AddAsync(userHashtag);
                    await _context.SaveChangesAsync();
                }


            }

        }

        public async Task ProcessHashtagsForRemovedPostAsync(string content, int loggedInUserId)
        {
            var postHashtags = HashtagsHelper.GetHashtags(content);

            foreach (var hashtag in postHashtags)
            {
                var hashtagDb = await _context.Hashtags.FirstOrDefaultAsync(h => h.Name == hashtag);
                if (hashtagDb != null)
                {
                    hashtagDb.Count--;
                    hashtagDb.DateUpdate = DateTime.Now;

                    if (hashtagDb.Count == 0)
                    {
                        _context.Hashtags.Remove(hashtagDb);
                    }
                    else
                    {
                        _context.Hashtags.Update(hashtagDb);
                    }

                    await _context.SaveChangesAsync();

                    // Remove UserHashtag entry if the count is zero
                    if (hashtagDb.Count == 0)
                    {
                        var userHashtag = await _context.UserHashtags
                            .FirstOrDefaultAsync(uh => uh.UserId == loggedInUserId && uh.HashtagId == hashtagDb.Id);

                        if (userHashtag != null)
                        {
                            _context.UserHashtags.Remove(userHashtag);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
        }
    }
}
