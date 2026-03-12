using MediatR;
using FranchiseService.Application.Features.Shifts.DTOs;

namespace FranchiseService.Application.Features.Shifts.Queries.GetActiveStaffsAtStore
{
    public class GetActiveStaffsAtStoreQuery : IRequest<List<ActiveStaffDto>>
    {
        public Guid StoreId { get; set; }

        public GetActiveStaffsAtStoreQuery(Guid storeId)
        {
            StoreId = storeId;
        }
    }

    public class ActiveStaffDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
