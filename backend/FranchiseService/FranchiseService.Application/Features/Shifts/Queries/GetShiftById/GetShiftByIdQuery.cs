using FranchiseService.Application.Features.Shifts.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetShiftById
{
    public class GetShiftByIdQuery : IRequest<ShiftResponse>
    {
        public Guid Id { get; set; }

        public GetShiftByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
