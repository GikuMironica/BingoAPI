using BingoAPI.Domain;
using ImageProcessor;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class ImageToWebpProcessor : IImageToWebpProcessor
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public ImageToWebpProcessor(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        public ImageProcessingResult ConvertFiles(List<IFormFile> images)
        {
            ImageProcessingResult resultList = new ImageProcessingResult { ProcessedPictures = new List<string>(), Result = true };
            
            foreach (var image in images)
            {
                // validate image format
                string[] allowedImageTypes = new string[] { "image/jpeg", "image/png", "image/jpg" };
                if (!allowedImageTypes.Contains(image.ContentType.ToLower()))
                {
                    resultList.Result = false;
                    resultList.ErrorMessage = "Image format not accepted. Accepted formats: jpeg / jpg / png";
                    return resultList;
                }

                string imagesPath = Path.Combine(webHostEnvironment.WebRootPath, "EventPics");
                string webPFileName = Guid.NewGuid().ToString() + ".webp";
                string webPImagePath = Path.Combine(imagesPath, webPFileName);

                //save names
                resultList.ProcessedPictures.Add(webPFileName);

                // convert pic in Main Memory
                using (var webPFileStream = new FileStream(webPImagePath, FileMode.Create))
                {
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                    {
                        imageFactory.Load(image.OpenReadStream())
                        .Format(new WebPFormat())
                        .Quality(100)
                        .Save(webPFileStream);
                    }
                }

            }
            return resultList;
        }
    }
}
