using BingoAPI.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class ImageLoader : IImageLoader
    {
        public async Task<ImageProcessingResult> LoadFiles(List<IFormFile> images)
        {
            ImageProcessingResult resultList = new ImageProcessingResult { ProcessedPictures = new List<MemoryStream>(), Result = true };

            foreach (var image in images)
            {
                // validate image format
                string[] allowedImageTypes = new string[] { "image/jpeg", "image/png", "image/jpg", "image/webp" };
                if (!allowedImageTypes.Contains(image.ContentType.ToLower()))
                {
                    resultList.Result = false;
                    resultList.ErrorMessage = "Image format not accepted. Accepted formats: jpeg / jpg / png / webp";
                    return resultList;
                }

                // load pic in Main Memory
                MemoryStream memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                resultList.ProcessedPictures.Add(memoryStream);                                   
                
            }

            return resultList;
        }
    }
}
