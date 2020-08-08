using Bingo.Contracts.V1.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class UpdatedTimeValidationResult
    {
        public bool Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}
