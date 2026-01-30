using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Categories.Queries.GetById
{
    public sealed class GetCategoryByIdValidator:
        AbstractValidator<GetCategoryByIdQuery>
    {
        public GetCategoryByIdValidator()
        {
            RuleFor(x => x.categoryId)
                .NotEmpty()
                .WithMessage("Category ID is required")
                .Must(categoryId => categoryId != Guid.Empty)
                .WithMessage("Category Id must be a valid non-empty GUID.");
        }
    }
}
