using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Features.Campaign.Commands.Create;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Validator.Campaign
{
    public class CreateCampaginValidator : AbstractValidator<CreateCampaginReq>
    {
        public CreateCampaginValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("StartDate is required");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("EndDate is required")
                .GreaterThan(x => x.StartDate)
                .WithMessage("EndDate must be greater than StartDate");

            RuleFor(x => x.VoucherIds)
                .NotNull().WithMessage("Vouchers cannot be null");
        }
    }
}
