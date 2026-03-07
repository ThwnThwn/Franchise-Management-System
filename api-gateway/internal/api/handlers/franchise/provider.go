package franchise

import "github.com/google/wire"

type FranchiseHandlers struct {
	*StoreHandler

	*ShiftHandler
	*ShiftApplicationHandler
	*ShiftStaffHandler
	*ShiftSwapHandler
	*ShiftAvailabilityHandler
	*ShiftBulkHandler
	*ShiftRuleHandler
	*ShiftTemplateHandler
}

func NewFranchiseHandlers(
	store *StoreHandler,
	shift *ShiftHandler,
	app *ShiftApplicationHandler,
	shiftStaff *ShiftStaffHandler,
	swap *ShiftSwapHandler,
	shiftAvailability *ShiftAvailabilityHandler,
	shiftBulk *ShiftBulkHandler,
	shiftRule *ShiftRuleHandler,
	shiftTemplate *ShiftTemplateHandler,
) FranchiseHandlers {
	return FranchiseHandlers{
		StoreHandler:             store,
		ShiftHandler:             shift,
		ShiftApplicationHandler:  app,
		ShiftStaffHandler:        shiftStaff,
		ShiftSwapHandler:         swap,
		ShiftAvailabilityHandler: shiftAvailability,
		ShiftBulkHandler:         shiftBulk,
		ShiftRuleHandler:         shiftRule,
		ShiftTemplateHandler:     shiftTemplate,
	}
}

var ProviderSet = wire.NewSet(
	NewStoreHandler,

	NewShiftHandler,
	NewShiftApplicationHandler,
	NewShiftStaffHandler,
	NewShiftSwapHandler,
	NewShiftAvailabilityHandler,
	NewShiftBulkHandler,
	NewShiftRuleHandler,
	NewShiftTemplateHandler,

	NewFranchiseHandlers,
)
