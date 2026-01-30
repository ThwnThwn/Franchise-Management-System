using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Products.Queries.GetById
{
    public sealed class GetProductByIdValidator:
        AbstractValidator<GetProductByIdQuery>
    {
        public GetProductByIdValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product Id must be provided.")
                .Must(id => id != Guid.Empty)
                .WithMessage("Product Id must be a valid non-empty GUID.");
        }
    }
}
