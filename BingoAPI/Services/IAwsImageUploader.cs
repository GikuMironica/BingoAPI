using BingoAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IAwsImageUploader
    {
        public Task<bool> UploadFileAsync(ImageProcessingResult imageProcessingResult);
    }
}
