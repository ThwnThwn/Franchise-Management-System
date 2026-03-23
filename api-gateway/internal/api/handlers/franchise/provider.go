package franchise

import "github.com/google/wire"

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
)
