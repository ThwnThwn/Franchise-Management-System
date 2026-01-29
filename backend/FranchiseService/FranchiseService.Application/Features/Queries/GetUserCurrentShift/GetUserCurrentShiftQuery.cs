using FranchiseService.Application.Features.Dtos;
using FranchiseService.Application.Repositories;
using MediatR;

namespace FranchiseService.Application.Features.Queries.GetUserCurrentShift
{
    public class GetUserCurrentShiftQuery : IRequest<UserShiftDto>
    {
        public string UserId { get; set; } = null!;
    }
}