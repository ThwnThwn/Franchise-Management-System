using FranchiseService.Application.Features.Dtos;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Queries.CheckStoreAvailability
{
    public class CheckStoreAvailabilityQuery : IRequest<AvailabilityResult>
    {
        public string? StoreId { get; set; }
    }
}