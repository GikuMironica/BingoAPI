using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses
{
    public class ErrorResponse
    {
        public List<ErrorModel> Errors { get; set; } = new List<ErrorModel>();
    }
}
