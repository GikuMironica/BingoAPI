using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class ImageUploadResult
    {
        public bool Result { get; set; }

        public List<string> ImageNames { get; set; }
    }
}
