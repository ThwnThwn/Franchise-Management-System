using FluentValidation;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.Category.Commands.UpdateCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Validators.Category
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator() 
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage(Message.Category.NameRequired)
                .MaximumLength(100).WithMessage(Message.Category.NameTooLong)
                .When(x => !string.IsNullOrEmpty(x.Request.Name));
        }
    }
}
