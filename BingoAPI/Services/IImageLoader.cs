using BingoAPI.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IImageLoader
    {
        public Task<ImageProcessingResult> LoadFiles(List<IFormFile> images);
    }
}
