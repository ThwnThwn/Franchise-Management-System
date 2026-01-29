using FranchiseService.Application.Features.Stores.Dtos;
using FranchiseService.Application.Repositories;
using MediatR;

namespace FranchiseService.Application.Features.Stores.Queries.FindNearbyStores
{
    public class FindNearbyStoresQuery : IRequest<List<StoreDto>>
    {
        public double UserLat { get; set; }
        public double UserLong { get; set; }
        public double RadiusKm { get; set; }
        public int Limit { get; set; }
    }
}