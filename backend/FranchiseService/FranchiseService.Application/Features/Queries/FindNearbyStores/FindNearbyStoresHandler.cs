using FranchiseService.Application.Features.Dtos;
using FranchiseService.Application.Repositories;
using MediatR;

namespace FranchiseService.Application.Features.Queries.FindNearbyStores
{
    public class FindNearbyStoresHandler : IRequestHandler<FindNearbyStoresQuery, List<StoreDto>>
    {
        private readonly IStoreRepository _storeRepo;

        public FindNearbyStoresHandler(IStoreRepository storeRepo)
        {
            _storeRepo = storeRepo;
        }

        public async Task<List<StoreDto>> Handle(FindNearbyStoresQuery request, CancellationToken cancellationToken)
        {
            var stores = await _storeRepo.GetNearbyStoresAsync(
                request.UserLat,
                request.UserLong,
                request.RadiusKm,
                request.Limit);

            // Map sang DTO
            return stores.Select(s => new StoreDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                Latitude = (double)(s.Latitude ?? 0),
                Longitude = (double)(s.Longitude ?? 0),
                ImageUrl = s.ImageUrl,
                IsOpenNow = true
            }).ToList();
        }
    }
}