using Bingo.Contracts.V1.Requests.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.CustomValidation
{
    public interface IUpdatedPostDetailsWatcher
    {
        public bool GetValidatedFields(UpdatePostRequest updatePostRequest);
    }

 
}
