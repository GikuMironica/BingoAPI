using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bingo.Contracts.V1.Requests.Bug
{
    public class CreateBugReport
    {
        [Required]
        [MaxLength(300)]
        public string Message { get; set; }

        public IList<IFormFile> Screenshots { get; set; }
    }
}
