using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Post
{
    public class DisablePostRequest
    {
        [Required]
        public int Id { get; set; }
    }
}
