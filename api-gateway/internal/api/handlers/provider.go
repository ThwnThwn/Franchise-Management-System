package handlers

import (
	"api-gateway/internal/api/handlers/calc"

	"github.com/google/wire"
)

// Gom nhóm tất cả Handler lại
var HandlerSet = wire.NewSet(
	calc.NewCalcHandler,
	NewAuthHandler,
)

type AppHandlers struct {
	CalcHandler *calc.CalcHandler
	AuthHandler *AuthHandler
}
