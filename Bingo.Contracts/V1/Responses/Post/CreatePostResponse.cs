using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.Post
{
    public class CreatePostResponse
    {
        public int Id { get; set; }

        public long PostTime { get; set; }

        public long EventTime { get; set; }

        public string UserId { get; set; }

        public IEnumerable<string>? Pictures { get; set; }

        public virtual List<string>? Tags { get; set; }
    }
}
