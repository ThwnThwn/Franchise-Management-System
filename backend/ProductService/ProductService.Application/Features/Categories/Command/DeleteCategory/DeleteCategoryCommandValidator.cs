using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Features.Categories.Command.DeleteCategory
{
    public sealed class DeleteCategoryCommandValidator :AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryCommandValidator()
        {
            RuleFor(x=>x.categoryId)
                .NotEmpty()
                .WithMessage("Category ID is required");
        }
    }
}
