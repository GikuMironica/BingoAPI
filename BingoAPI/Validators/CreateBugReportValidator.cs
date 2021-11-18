using Bingo.Contracts.V1.Requests.Bug;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Validators
{
    public class CreateBugReportValidator : AbstractValidator<CreateBugReport>
    {
        public CreateBugReportValidator()
        {
            this.CascadeMode = CascadeMode.Stop;

            RuleFor(b => b.Screenshots.Count)
                .LessThanOrEqualTo(3)
                .WithMessage("Can't upload more than 3 screenshots");
        }
    }
}
