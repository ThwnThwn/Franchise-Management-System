using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Products.Queries.GetByProductName
{
    internal class GetProductByNameQueryValidator :
        AbstractValidator<GetProductByNameQuery>
    {
        public GetProductByNameQueryValidator()
        {
            RuleFor(x => x.productName)
                .NotNull()
                .WithMessage("Product name is required.");
        }
    }
}
