package franchise

import (
	franchiseGW "api-gateway/internal/application/gateways/franchise"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/shift"
	"context"

	"api-gateway/pkg/mapper"
	franchisePb "api-gateway/pkg/pb/franchise"
	"api-gateway/pkg/utils/stringutils"

	"github.com/google/uuid"
)

type shiftGrpcClient struct {
	client franchisePb.ShiftGrpcServiceClient
}

func ProvideShiftClient(f *clientFactory) franchiseGW.IShiftService {
	return &shiftGrpcClient{client: franchisePb.NewShiftGrpcServiceClient(f.conn)}
}

// GetUserCurrentShift implements franchise.IShiftService.
func (s *shiftGrpcClient) GetUserCurrentShift(ctx context.Context, id uuid.UUID) (*shift.GetUserCurrentShiftResponse, error) {
	res, err := s.client.GetUserCurrentShift(ctx, &franchisePb.GetUserCurrentShiftRequest{
		UserId: id.String(),
	})
	if err != nil {
		return nil, err
	}

	return &shift.GetUserCurrentShiftResponse{
		HasShiftToday: res.HasShiftToday,

		// IDs (Optional)
		AssignmentId: res.AssignmentId,
		ShiftId:      res.ShiftId,
		CurrentLogId: res.CurrentLogId,

		// Cửa hàng
		StoreId:   res.StoreId,
		StoreName: res.StoreName,
		StoreLat:  res.StoreLat,  // Map con trỏ *float64
		StoreLong: res.StoreLong, // Map con trỏ *float64

		// Thời gian ca làm
		StartTime: res.StartTime,
		EndTime:   res.EndTime,

		// Logic Flags
		CanCheckIn:  res.CanCheckIn,
		CanCheckOut: res.CanCheckOut,
		IsOvertime:  res.IsOvertime,

		// Timestamps (Optional)
		CheckInAt:  res.CheckInAt,
		CheckOutAt: res.CheckOutAt,
		ServerTime: res.ServerTime,
	}, nil
}

// GetShifts implements franchise.IShiftService.
func (s *shiftGrpcClient) GetShifts(ctx context.Context, req *shift.GetShiftsParams) ([]*shift.ShiftResponse, error) {
	var statusValue int32 = -1
	if req.Status != nil {
		statusValue = (int32)(*req.Status)
	}

	res, err := s.client.GetShifts(ctx, &franchisePb.GetShiftsRequest{
		StoreId:  req.StoreID,
		FromDate: req.FromDate,
		ToDate:   req.ToDate,
		Status:   statusValue,
	})
	if err != nil {
		return []*shift.ShiftResponse{}, err
	}

	shifts := make([]*shift.ShiftResponse, 0, len(res.Data))
	for _, item := range res.Data {
		shifts = append(shifts, mapper.MapShiftPbToModel(item))
	}
	return shifts, nil
}

// GetById implements franchise.IShiftService.
func (s *shiftGrpcClient) GetById(ctx context.Context, id uuid.UUID) (*shift.ShiftResponse, error) {
	res, err := s.client.GetShiftById(ctx, &franchisePb.GetShiftByIdRequest{
		Id: id.String(),
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapShiftPbToModel(res), nil
}

// Create implements franchise.IShiftService.
func (s *shiftGrpcClient) Create(ctx context.Context, creatorId uuid.UUID, req *shift.CreateShiftRequest) error {
	_, err := s.client.CreateShift(ctx, &franchisePb.CreateShiftRequest{
		StoreId:              req.StoreID,
		ShiftTemplateId:      req.ShiftTemplateID,
		ShiftDate:            req.ShiftDate,
		StartTime:            req.StartTime,
		EndTime:              req.EndTime,
		RequiredStaffCount:   int32(req.RequiredStaffCount),
		BreakDurationMinutes: int32(req.BreakDurationMinutes),
		Note:                 *req.Note,
		CreatedBy:            creatorId.String(),
	})
	return err
}

// Update implements franchise.IShiftService.
func (s *shiftGrpcClient) Update(ctx context.Context, id, userId uuid.UUID, req *shift.UpdateShiftRequest) error {
	var statusValue int32 = -1
	if req.Status != nil {
		statusValue = (int32)(*req.Status)
	}

	_, err := s.client.UpdateShift(ctx, &franchisePb.UpdateShiftRequest{
		// StoreId:              req.StoreID,
		Id:                   id.String(),
		ShiftTemplateId:      req.ShiftTemplateID,
		ShiftDate:            req.ShiftDate,
		StartTime:            req.StartTime,
		EndTime:              req.EndTime,
		RequiredStaffCount:   req.RequiredStaffCount,
		BreakDurationMinutes: req.BreakDurationMinutes,
		Note:                 req.Note,
		Status:               statusValue,
		UpdatedBy:            userId.String(),
	})
	return err
}

// Delete implements franchise.IShiftService.
func (s *shiftGrpcClient) Delete(ctx context.Context, id uuid.UUID) error {
	_, err := s.client.DeleteShift(ctx, &franchisePb.DeleteShiftRequest{
		Id: id.String(),
	})
	return err
}

// Publish implements franchise.IShiftService.
func (s *shiftGrpcClient) Publish(ctx context.Context, id uuid.UUID) error {
	_, err := s.client.PublishShift(ctx, &franchisePb.PublishShiftRequest{
		Id: id.String(),
	})
	return err
}

// UpdateAssignments implements franchise.IShiftService.
func (s *shiftGrpcClient) UpdateAssignments(ctx context.Context, id uuid.UUID, userIds []uuid.UUID) error {
	userIdsStr := make([]string, 0, len(userIds))
	for _, u := range userIds {
		userIdsStr = append(userIdsStr, u.String())
	}

	_, err := s.client.AssignStaff(ctx, &franchisePb.AssignStaffRequest{
		ShiftId: id.String(),
		UserIds: userIdsStr,
	})
	return err
}

// GetAvailableStaffs implements franchise.IShiftService.
func (s *shiftGrpcClient) GetAvailableStaffs(ctx context.Context, storeId uuid.UUID, shiftId uuid.UUID) ([]*shift.StaffAssignmentCandidate, error) {
	res, err := s.client.GetAvailableStaffs(ctx, &franchisePb.GetAvailableStaffsRequest{
		ShiftId: shiftId.String(),
		StoreId: storeId.String(),
	})
	if err != nil {
		return []*shift.StaffAssignmentCandidate{}, err
	}

	staffs := make([]*shift.StaffAssignmentCandidate, 0, len(res.Data))
	for _, item := range res.Data {
		staffs = append(staffs, &shift.StaffAssignmentCandidate{
			ID:                item.Id,
			FirstName:         item.FirstName,
			LastName:          stringutils.ToPtr(item.LastName),
			AvatarURL:         stringutils.ToPtr(item.AvatarUrl),
			CurrentHourlyWage: item.CurrentHourlyWage,
			IsAvailable:       item.IsAvailable,
			Note:              stringutils.ToPtr(item.Note),
			IsAssigned:        item.IsAssigned,
			IsConflict:        item.IsConflict,
			Validation: &shift.CandidateValidation{
				IsRestTimeInvalid: item.Validation.IsRestTimeInvalid,
				RestHours:         item.Validation.RestHours,
				IsOverLimit:       item.Validation.IsOverLimit,
				WeeklyHours:       item.Validation.WeeklyHours,
				IsShuttleInvalid:  item.Validation.IsShuttleInvalid,
			},
		})
	}

	return staffs, nil
}

// CheckIn implements franchise.IShiftService.
func (s *shiftGrpcClient) CheckIn(ctx context.Context, userId uuid.UUID, assignmentId uuid.UUID, req *shift.AttendancePayload) error {
	_, err := s.client.CheckIn(ctx, &franchisePb.CheckInRequest{
		AssignmentId: assignmentId.String(),
		UserId:       userId.String(),
		Latitude:     req.Latitude,
		Longitude:    req.Longitude,
		Note:         stringutils.GetString(req.Note),
	})
	return err
}

// CheckOut implements franchise.IShiftService.
func (s *shiftGrpcClient) CheckOut(ctx context.Context, userId uuid.UUID, logId uuid.UUID, req *shift.AttendancePayload) error {
	_, err := s.client.CheckOut(ctx, &franchisePb.CheckOutRequest{
		LogId:     logId.String(),
		UserId:    userId.String(),
		Latitude:  req.Latitude,
		Longitude: req.Longitude,
		Note:      stringutils.GetString(req.Note),
	})
	return err
}

// GetTimeLogs implements franchise.IShiftService.
func (s *shiftGrpcClient) GetTimeLogs(ctx context.Context, req *shift.GetTimeLogsParams) (common.PaginationResult[*shift.TimeLogResponse], error) {
	var statusValue int32 = -1
	if req.Status != nil {
		statusValue = (int32)(*req.Status)
	}

	res, err := s.client.GetTimeLogs(ctx, &franchisePb.GetTimeLogsRequest{
		StoreId:  stringutils.GetString(req.StoreID),
		StaffId:  stringutils.GetString(req.StaffID),
		Status:   statusValue,
		FromDate: req.FromDate,
		ToDate:   req.ToDate,
		Page:     req.Page,
		PageSize: req.PageSize,
	})
	if err != nil {
		return common.PaginationResult[*shift.TimeLogResponse]{}, err
	}

	logs := make([]*shift.TimeLogResponse, 0, len(res.Data))
	for _, item := range res.Data {
		logs = append(logs, &shift.TimeLogResponse{
			ID:                item.Id,
			ShiftAssignmentID: item.ShiftAssignmentId,

			CheckInAt:       item.CheckInAt,
			CheckInImageURL: item.CheckInImageUrl,
			CheckInLat:      item.CheckInLat,
			CheckInLong:     item.CheckInLong,

			CheckOutAt:       item.CheckOutAt,
			CheckOutImageURL: item.CheckOutImageUrl,
			CheckOutLat:      item.CheckOutLat,
			CheckOutLong:     item.CheckOutLong,

			// TotalWorkMinutes: item.TotalWorkMinutes,
			Status:      item.Status,
			ManagerNote: item.ManagerNote,

			CreatedAt: item.CreatedAt,
			UpdatedAt: item.UpdatedAt,

			Staff: shift.StaffTimeLogRes{
				Id:        item.Staff.Id,
				FirstName: item.Staff.FirstName,
				LastName:  item.Staff.LastName,
				AvatarUrl: item.Staff.AvatarUrl,
				Email:     item.Staff.Email,
			},

			Shift: shift.ShiftTimeLogRes{
				Id:        item.Shift.Id,
				ShiftDate: item.Shift.ShiftDate,
				StartTime: item.Shift.StartTime,
				EndTime:   item.Shift.EndTime,
				Note:      item.Shift.Note,
			},
		})
	}

	return common.PaginationResult[*shift.TimeLogResponse]{
		Items:      logs,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
}

// UpdateTimeLog implements franchise.IShiftService.
func (s *shiftGrpcClient) UpdateTimeLog(ctx context.Context, id uuid.UUID, req *shift.UpdateTimeLogRequest) error {
	_, err := s.client.UpdateTimeLog(ctx, &franchisePb.UpdateTimeLogRequest{
		Id:          id.String(),
		Status:      int32(req.Status),
		ManagerNote: req.ManagerNote,
	})
	return err
}

// GetMyAgenda implements franchise.IShiftService.
func (s *shiftGrpcClient) GetMyAgenda(ctx context.Context, userId uuid.UUID, req *shift.GetAgendaParams) ([]*shift.StaffShiftAgenda, error) {
	res, err := s.client.GetMyAgenda(ctx, &franchisePb.GetAgendaRequest{
		UserId:    userId.String(),
		StartDate: req.StartDate,
		EndDate:   req.EndDate,
	})
	if err != nil {
		return []*shift.StaffShiftAgenda{}, err
	}

	shifts := make([]*shift.StaffShiftAgenda, 0, len(res.Data))
	for _, item := range res.Data {
		coWorkers := make([]shift.CoWorkerDTO, 0, len(item.CoWorkers))
		for _, cw := range item.CoWorkers {
			coWorkers = append(coWorkers, shift.CoWorkerDTO{
				ID:        cw.Id,
				FirstName: cw.FirstName,
				LastName:  stringutils.ToPtr(cw.LastName),
				Avatar:    stringutils.ToPtr(cw.Avatar),
				Phone:     stringutils.ToPtr(cw.Phone),
			})
		}

		shifts = append(shifts, &shift.StaffShiftAgenda{
			ID:        item.Id,
			StartTime: item.StartTime,
			EndTime:   item.EndTime,
			Date:      item.Date,
			Store:     *mapper.MapAgendaStorePbToModel(item.Store),

			CheckInAt:  item.CheckInAt,
			CheckOutAt: item.CheckOutAt,

			CoWorkers: coWorkers,
		})
	}

	return shifts, nil
}

// BulkCreateManual implements franchise.IShiftService.
func (s *shiftGrpcClient) BulkCreateManual(ctx context.Context, creatorId uuid.UUID, req *shift.BulkCreateManualRequest) error {
	_, err := s.client.BulkCreateManual(ctx, &franchisePb.BulkCreateManualRequest{
		StoreId:              req.StoreID,
		StartTime:            req.StartTime,
		EndTime:              req.EndTime,
		DaysOfWeek:           req.DaysOfWeek,
		StartDate:            req.StartDate,
		EndDate:              req.EndDate,
		RequiredStaffCount:   req.RequiredStaffCount,
		BreakDurationMinutes: req.BreakDurationMinutes,
		Note:                 stringutils.GetString(req.Note),
		CreatedBy:            creatorId.String(),
	})
	return err
}

// BulkCreateFromTemplate implements franchise.IShiftService.
func (s *shiftGrpcClient) BulkCreateFromTemplate(ctx context.Context, creatorId uuid.UUID, req *shift.BulkCreateFromTemplateRequest) error {
	_, err := s.client.BulkCreateFromTemplate(ctx, &franchisePb.BulkCreateFromTemplateRequest{
		StoreId:            req.StoreID,
		TemplateId:         req.TemplateID,
		StartDate:          req.StartDate,
		EndDate:            req.EndDate,
		RequiredStaffCount: req.RequiredStaffCount,
		CreatedBy:          creatorId.String(),
	})
	return err
}

// BulkDelete implements franchise.IShiftService.
func (s *shiftGrpcClient) BulkDelete(ctx context.Context, shiftIds []string) error {
	_, err := s.client.BulkDelete(ctx, &franchisePb.BulkDeleteRequest{
		Ids: shiftIds,
	})
	return err
}

// BulkPublish implements franchise.IShiftService.
func (s *shiftGrpcClient) BulkPublish(ctx context.Context, shiftIds []string) error {
	_, err := s.client.BulkPublish(ctx, &franchisePb.BulkPublishRequest{
		Ids: shiftIds,
	})
	return err
}
