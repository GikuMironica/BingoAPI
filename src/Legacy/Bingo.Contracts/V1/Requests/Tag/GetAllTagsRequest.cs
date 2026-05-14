using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Tag
{
    public class GetAllTagsRequest
    {
        [Required]
        [MinLength(2)]
        public string TagName { get; set; }
    }
}
