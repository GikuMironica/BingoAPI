using Bingo.Contracts.V1.Requests.User;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Validators
{
    /// <summary>
    /// Define validation rules for UpdateUserRequest,
    /// Requires filters midleware to translate the error message
    /// </summary>
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
        
    }
}
