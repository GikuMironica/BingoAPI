using Bingo.Contracts.V1.Requests.Identity;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Validators
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator() 
        {
            RuleFor(x => x.email)
                .NotNull()
                .EmailAddress();

            RuleFor(x => x.token)
                .NotNull();
        }
    }
}
