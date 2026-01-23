package handlers

import "github.com/google/wire"

// Gom nhóm tất cả Handler lại
var HandlerSet = wire.NewSet(
	NewCalcHandler,
)

type AppHandlers struct {
	CalcHandler *CalcHandler
}
