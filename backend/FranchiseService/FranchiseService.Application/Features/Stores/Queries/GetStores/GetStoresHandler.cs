using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Features.Stores.Dtos;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using MediatR;

namespace FranchiseService.Application.Features.Stores.Queries.GetStores
{
    public class GetStoresHandler : IRequestHandler<GetStoresQuery, PagedResult<StoreDto>>
    {
        private readonly IStoreRepository _repo;

        public GetStoresHandler(IStoreRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<StoreDto>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
        {
            var pagedData = await _repo.GetPagedStoresAsync(
                request.Keyword ?? "",
                request.Page,
                request.PageSize,
                cancellationToken);

            var dtos = pagedData.Items.Select(s => new StoreDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                Phone = s.Phone,
                ImageUrl = s.ImageUrl,
                Latitude = (double)(s.Latitude ?? 0),
                Longitude = (double)(s.Longitude ?? 0),
                IsOpenNow = s.Status == (int)StoreStatus.Open
            }).ToList();

            return new PagedResult<StoreDto>(dtos, pagedData.TotalCount, pagedData.Page, pagedData.PageSize);
        }
    }
}