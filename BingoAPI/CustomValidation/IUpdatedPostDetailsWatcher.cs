using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Domain;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.CustomValidation
{
    public interface IUpdatedPostDetailsWatcher
    {
        public bool GetValidatedFields(UpdatePostRequest updatePostRequest);

        public UpdatedTimeValidationResult ValidateUpdatedTime(UpdatePostRequest updatePostRequest, Post post);
    }

 
}
