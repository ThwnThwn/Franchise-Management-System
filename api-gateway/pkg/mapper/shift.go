package mapper

import (
	"api-gateway/internal/domain/models/shift"
	"api-gateway/internal/domain/models/user"
	"api-gateway/pkg/pb/franchise"
	"api-gateway/pkg/utils/stringutils"
)

// MapStaffToTimeLogsBatch xử lý nhồi thông tin nhân viên vào danh sách nhật ký
func MapStaffToTimeLogsBatch(logs []*shift.TimeLogResponse, users []*user.UserResponse) []*shift.TimeLogResponse {
	if len(logs) == 0 {
		return logs
	}

	// 1. Chuyển []*user.UserResponse thành map[string]*user.UserResponse
	// Mục đích: Tránh vòng lặp lồng nhau (O(N*M) -> O(N+M))
	userMap := make(map[string]*user.UserResponse, len(users))
	for _, u := range users {
		if u != nil {
			userMap[u.Id] = u
		}
	}

	// 2. Duyệt qua danh sách logs và map dữ liệu
	for _, log := range logs {
		if log == nil {
			continue
		}

		staffID := log.Staff.Id
		// Tìm nhân viên trong map dựa trên Staff.ID của log
		if u, ok := userMap[staffID]; ok {
			// Vì log và u đều là pointer nên truyền thẳng vào hàm Map lẻ
			MapStaffToTimeLog(log, u)
		}
	}

	return logs
}

// MapStaffToTimeLog (Hàm lẻ để bro đối chiếu)
func MapStaffToTimeLog(log *shift.TimeLogResponse, u *user.UserResponse) {
	if u == nil || log == nil {
		return
	}

	log.Staff.Id = u.Id
	log.Staff.FirstName = stringutils.ToPtr(u.FirstName)
	log.Staff.LastName = stringutils.ToPtr(u.LastName)
	log.Staff.Email = stringutils.ToPtr(u.Email)
}

func MapShiftPbToModel(shiftPb *franchise.ShiftResponse) *shift.ShiftResponse {
	assignments := make([]*shift.ShiftAssignment, 0, len(shiftPb.Assignments))
	for _, item := range shiftPb.Assignments {
		assignments = append(assignments, &shift.ShiftAssignment{
			ID:         item.Id,
			ShiftID:    item.ShiftId,
			UserID:     item.UserId,
			Status:     int(item.Status),
			HourlyWage: item.HourlyWage,

			FirstName: item.FirstName,
			LastName:  item.LastName,
			AvatarURL: item.AvatarUrl,
			Email:     &item.Email,
		})
	}

	return &shift.ShiftResponse{
		ID:                   shiftPb.Id,
		StoreID:              shiftPb.StoreId,
		ShiftDate:            shiftPb.ShiftDate,
		StartTime:            shiftPb.StartTime,
		EndTime:              shiftPb.EndTime,
		RequiredStaffCount:   int(shiftPb.RequiredStaffCount),
		BreakDurationMinutes: int(shiftPb.BreakDurationMinutes),
		ShiftTemplateID:      shiftPb.ShiftTemplateId,
		Status:               int(shiftPb.Status),
		Note:                 shiftPb.Note,
		AssignedStaffCount:   int(shiftPb.AssignedStaffCount),
	}
}
