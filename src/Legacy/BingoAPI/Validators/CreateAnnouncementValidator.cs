using Bingo.Contracts.V1.Requests.Announcement;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Validators
{
    public class CreateAnnouncementValidator : AbstractValidator<CreateAnnouncementRequest>
    {
        public CreateAnnouncementValidator()
        {
            // this.CascadeMode = CascadeMode.StopOnFirstFailure;
            RuleFor(x => x.Message)
                 .MinimumLength(1)
                 .MaximumLength(500);
        }
    }
}
