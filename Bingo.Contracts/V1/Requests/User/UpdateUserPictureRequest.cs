using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.User
{
    public class UpdateUserPictureRequest
    {
        [Required]
        public IFormFile UpdatedPicture { get; set; }
    }
}
