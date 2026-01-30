using Ardalis.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProductService.Application.Features.Categories.Queries.GetByCategoryName.GetCategoryByNameQuery;

namespace ProductService.Application.Features.Categories.Queries.GetByCategoryName
{
    public sealed record GetCategoryByNameQuery(string categoryName) :
       IRequest<Result<GetCategoryByNameQueryResponse>>
    {
        public sealed class GetCategoryByNameQueryResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = default!;
            public string? Description { get; set; } 
            public bool? IsActive { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset? UpdatedAt { get; set; }
        }
    }
}
