using FranchiseService.Application.Features.Shifts.Dtos;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetUserCurrentShift
{
    public class GetUserCurrentShiftQuery : IRequest<UserShiftDto>
    {
        public string UserId { get; set; } = null!;
    }
}