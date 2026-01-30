using Ardalis.Result;
using MediatR;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProductService.Application.Features.Categories.Command.CreateCategory.CreateCategoryCommand;

namespace ProductService.Application.Features.Categories.Command.CreateCategory
{
    public sealed record CreateCategoryCommand(CreateCategoryCommandRequest Request):
        IRequest<Result<CreateCategoryCommandResponse>>
    {
        public sealed class CreateCategoryCommandRequest
        {
            public string Name { get; set; } = default!;
            public string? Description { get; set; }
        }

        public sealed class CreateCategoryCommandResponse {
            public Guid Id { get; set; }
            public string Name { get; set; } = default!;
            public string? Description { get; set; } = default!;
            public bool? IsActive { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }
    }
}
