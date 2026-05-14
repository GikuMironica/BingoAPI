using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.Rating
{
    public class GetRating
    {
        public int Rate { get; set; }

        public String? Feedback { get; set; }
    }
}
