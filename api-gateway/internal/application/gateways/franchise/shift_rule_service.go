package franchise

import (
	"api-gateway/internal/domain/models/shift"
	"context"

	"github.com/google/uuid"
)

type IShiftRuleService interface {
	GetSettings(ctx context.Context, storeId *uuid.UUID) (shift.ShiftRuleSettingsResponse, error)
	Save(ctx context.Context, userId uuid.UUID, req *shift.SaveShiftRuleRequest) error
}
