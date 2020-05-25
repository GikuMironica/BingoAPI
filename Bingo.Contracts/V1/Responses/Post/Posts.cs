using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.Post
{
    public class Posts
    {
        public int PostId { get; set; }
        public int PostType { get; set; }
        public string Thumbnail { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }

    }
}
