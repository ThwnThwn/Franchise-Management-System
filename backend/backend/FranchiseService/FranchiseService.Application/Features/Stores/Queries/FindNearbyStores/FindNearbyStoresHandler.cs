using FranchiseService.Application.Features.Stores.Dtos;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Stores.Queries.FindNearbyStores
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
                Phone = s.Phone,
                ImageUrl = s.ImageUrl,
                Latitude = (double?)s.Latitude,
                Longitude = (double?)s.Longitude,
                IsOpenNow = s.Status == (int)StoreStatus.Open
            }).ToList();
        }
    }
}