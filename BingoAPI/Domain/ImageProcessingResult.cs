using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class ImageProcessingResult
    {
        public bool Result { get; set; }

        public string ErrorMessage { get; set; }

        public List<Image> ProcessedPictures { get; set; }
    }
}
