package franchise

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/shift"
	"context"

	"github.com/google/uuid"
)

type IShiftService interface {
	// Lấy lịch làm việc hôm nay của nhân viên (Để App biết đang làm ở Store nào)
	GetUserCurrentShift(ctx context.Context, id uuid.UUID) (
		*shift.UserCurrentShiftResponse, error)
	GetActiveStaffsAtStore(ctx context.Context, storeId uuid.UUID) ([]*shift.ActiveStaffDto, error)

	GetShifts(ctx context.Context, req *shift.GetShiftsParams) ([]*shift.ShiftResponse, error)
	GetById(ctx context.Context, id uuid.UUID) (*shift.ShiftResponse, error)
	Create(ctx context.Context, creatorId uuid.UUID, req *shift.CreateShiftRequest) error
	Update(ctx context.Context, id, userId uuid.UUID, req *shift.UpdateShiftRequest) error
	Delete(ctx context.Context, id uuid.UUID) error

	Publish(ctx context.Context, id uuid.UUID) error
	UpdateAssignments(ctx context.Context, ctxUserId, id uuid.UUID, req *shift.AssignStaffRequest) (
		*shift.AssignStaffResponse, error)
	GetAvailableStaffs(ctx context.Context, storeId, shiftId uuid.UUID) ([]*shift.StaffAssignmentCandidate, error)

	CheckIn(ctx context.Context, userId, assignmentId uuid.UUID, req *shift.AttendancePayload) error
	CheckOut(ctx context.Context, userId, logId uuid.UUID, req *shift.AttendancePayload) error

	GetTimeLogs(ctx context.Context, req *shift.GetTimeLogsParams) (
		common.PaginationResult[*shift.TimeLogResponse], error)
	UpdateTimeLog(ctx context.Context, id uuid.UUID, req *shift.UpdateTimeLogRequest) error

	GetMyAgenda(ctx context.Context, userId uuid.UUID, req *shift.GetAgendaParams) ([]*shift.StaffShiftAgenda, error)

	BulkCreateManual(ctx context.Context, creatorId uuid.UUID, req *shift.BulkCreateManualRequest) error
	BulkCreateFromTemplate(ctx context.Context, creatorId uuid.UUID, req *shift.BulkCreateFromTemplateRequest) error
	BulkDelete(ctx context.Context, shiftIds []string) error
	BulkPublish(ctx context.Context, shiftIds []string) error
	BulkOpen(ctx context.Context, storeId uuid.UUID, shiftIds []string) (int32, error)

	// --- TU DONG XEP CA ---
	// He thong tu dong tim nhan vien phu hop va gan vao cac ca Draft chua du nguoi
	AutoFill(ctx context.Context, storeId uuid.UUID, req *shift.AutoFillShiftsRequest) (
		*shift.AutoFillShiftsResponse, error)
}
