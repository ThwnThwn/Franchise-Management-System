using FluentValidation;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.Category.Commands.CreateCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Validators.Category
{
    internal class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage(Message.Category.NameRequired)
                .MaximumLength(100).WithMessage(Message.Category.NameTooLong);
        }
    }
}
