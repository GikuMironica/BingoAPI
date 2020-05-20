using BingoAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IAwsBucketManager
    {
        public Task<ImageUploadResult> UploadFileAsync(ImageProcessingResult imageProcessingResult);

        public Task<ImageDeleteResult> DeleteFileAsync(List<string>? imagesGuids);
    }
}
