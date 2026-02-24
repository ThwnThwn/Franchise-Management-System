package franchise

import "github.com/google/wire"

type FranchiseHandlers struct {
	*StoreHandler
	*ShiftHandler
	*ShiftStaffHandler
	*ShiftAvailabilityHandler
	*ShiftBulkHandler
	*ShiftRuleHandler
	*ShiftTemplateHandler
}

func NewFranchiseHandlers(
	store *StoreHandler,
	shift *ShiftHandler,
	shiftStaff *ShiftStaffHandler,
	shiftAvailability *ShiftAvailabilityHandler,
	shiftBulk *ShiftBulkHandler,
	shiftRule *ShiftRuleHandler,
	shiftTemplate *ShiftTemplateHandler,
) FranchiseHandlers {
	return FranchiseHandlers{
		StoreHandler:             store,
		ShiftHandler:             shift,
		ShiftStaffHandler:        shiftStaff,
		ShiftAvailabilityHandler: shiftAvailability,
		ShiftBulkHandler:         shiftBulk,
		ShiftRuleHandler:         shiftRule,
		ShiftTemplateHandler:     shiftTemplate,
	}
}

var ProviderSet = wire.NewSet(
	NewStoreHandler,

	NewShiftHandler,
	NewShiftStaffHandler,
	NewShiftAvailabilityHandler,
	NewShiftBulkHandler,
	NewShiftRuleHandler,
	NewShiftTemplateHandler,

	NewFranchiseHandlers,
)
