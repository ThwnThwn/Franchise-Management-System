package franchise

import "github.com/google/wire"

type FranchiseHandlers struct {
	*StoreHandler
	*ShiftHandler
	*ShiftStaffHandler
}

func NewFranchiseHandlers(
	store *StoreHandler,
	shift *ShiftHandler,
	shiftStaff *ShiftStaffHandler,
) FranchiseHandlers {
	return FranchiseHandlers{
		StoreHandler:      store,
		ShiftHandler:      shift,
		ShiftStaffHandler: shiftStaff,
	}
}

var ProviderSet = wire.NewSet(
	NewStoreHandler,
	NewShiftHandler,
	NewShiftStaffHandler,

	NewFranchiseHandlers,
)
