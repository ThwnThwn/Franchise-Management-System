using Ardalis.Result;
using MediatR;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Categories.Command.UpdateCategory
{
    public sealed record UpdateCategoryCommand(Guid id, UpdateCategoryCommandRequest Request) 
        :IRequest<Result<UpdateCategoryCommandResponse>>;
    
        public sealed class UpdateCategoryCommandRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public bool? IsActive { get; set; }
        public string? Description { get; set; }
    }

        public sealed class UpdateCategoryCommandResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; } 
        public bool? IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
    }

