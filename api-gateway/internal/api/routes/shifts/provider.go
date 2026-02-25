package shifts

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers/franchise"
	"api-gateway/internal/api/middleware"
)

type ShiftsRouter struct {
	cfg            *config.Config
	authMW         *middleware.AuthMiddleware
	shiftHl        *franchise.ShiftHandler
	availabilityHl *franchise.ShiftAvailabilityHandler
	bulkHl         *franchise.ShiftBulkHandler
	ruleHl         *franchise.ShiftRuleHandler
	templateHl     *franchise.ShiftTemplateHandler // Mảnh ghép cuối cùng
}

func NewRouter(
	cfg *config.Config,
	authMW *middleware.AuthMiddleware,
	shiftHl *franchise.ShiftHandler,
	availabilityHl *franchise.ShiftAvailabilityHandler,
	bulkHl *franchise.ShiftBulkHandler,
	ruleHl *franchise.ShiftRuleHandler,
	templateHl *franchise.ShiftTemplateHandler,
) *ShiftsRouter {
	return &ShiftsRouter{
		cfg:            cfg,
		authMW:         authMW,
		shiftHl:        shiftHl,
		availabilityHl: availabilityHl,
		bulkHl:         bulkHl,
		ruleHl:         ruleHl,
		templateHl:     templateHl,
	}
}
