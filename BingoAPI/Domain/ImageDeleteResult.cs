using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class ImageDeleteResult
    {
        public bool Result { get; set; }
        public List<DeleteError> ErrorMessages { get; set; }
    }
}
