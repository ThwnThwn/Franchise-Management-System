using FranchiseService.Application.Features.Shifts.DTOs;
using BuildingBlocks.Core.Pagination;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetShifts
{
    public class GetShiftsQuery : IRequest<IEnumerable<ShiftResponse>>
    {
        public Guid StoreId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }

        //public int PageNumber { get; set; }
        //public int PageSize { get; set; }
        public int? Status { get; set; }

        public GetShiftsQuery(Guid storeId, DateOnly? fromDate, DateOnly? toDate,
            //int pageNumber, int pageSize,
            int? status)
        {
            StoreId = storeId;
            FromDate = fromDate;
            ToDate = toDate;
            //PageNumber = pageNumber;
            //PageSize = pageSize;
            Status = status;
        }
    }
}