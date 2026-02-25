using MediatR;

namespace FranchiseService.Application.Features.Shifts.Commands.UpdateShiftAssignment
{
    // Command dong bo danh sach nhan vien trong ca lam viec (Sync Logic)
    // FE gui: PUT /shifts/{id}/assign [userIds]
    // Mang userIds la ket qua cuoi cung: co trong request thi them, khong co thi xoa
    public record UpdateShiftAssignmentCommand : IRequest<Unit>
    {
        // ID ca lam viec (set tu route param boi Controller)
        public Guid ShiftId { get; set; }

        // Danh sach ID nhan vien cuoi cung sau khi Manager da chinh sua
        // Day la trang thai mong muon, BE se tu tinh toan them/xoa
        public List<Guid> UserIds { get; set; } = new();
    }
}
