using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EtherApp.Data.Services.Implementations
{
    public class FilesService : IFilesService
    {
        public async Task<string> UploadImageAsync(IFormFile file, ImageFileType imageFileType)
        {

            string filePathUpload = imageFileType switch
            {
                ImageFileType.PostImage => "images/Uploaded/PostImages",
                ImageFileType.StoryImage => "images/Uploaded/StoryImages",
                ImageFileType.ProfileImage => "images/Uploaded/ProfileImages",
                ImageFileType.CoverImage => "images/Uploaded/CoverImages",
                _ => throw new ArgumentException("Invalid image file type"),
            };

            if (file != null && file.Length > 0)
            {
                string rootFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (file.ContentType.Contains("image"))
                {
                    string rootFolderPathImages = Path.Combine(rootFolderPath, filePathUpload);
                    if (!Directory.Exists(rootFolderPathImages))
                    {
                        Directory.CreateDirectory(rootFolderPathImages);
                    }
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(rootFolderPathImages, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return $"{filePathUpload}/{fileName}";
                }
            }

            return string.Empty;
        }
    }
}
