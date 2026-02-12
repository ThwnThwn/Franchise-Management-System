using FranchiseService.Application.Features.Shifts.DTOs;
using MediatR;

namespace FranchiseService.Application.Features.Shifts.Queries.GetAvailableStaffs
{
    // Query lay danh sach nhan vien co the gan vao ca lam
    // Tra ve kem trang thai validation (trung lich, nghi ngoi, gioi han gio)
    // FE: GET /stores/{storeId}/shifts/{shiftId}/candidates
    public record GetAvailableStaffsQuery : IRequest<List<StaffAssignmentCandidateDto>>
    {
        // ID cua cua hang de gioi han danh sach nhan vien
        public Guid StoreId { get; init; }

        // ID cua ca lam de check conflict/availability/assigned
        public Guid ShiftId { get; init; }

        public GetAvailableStaffsQuery(Guid storeId, Guid shiftId)
        {
            StoreId = storeId;
            ShiftId = shiftId;
        }
    }
}
