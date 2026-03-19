package vnpay

import "github.com/google/wire"

var ProviderSet = wire.NewSet(
	NewVnPayHandler,

	// NewVnPayHandlers,
)

// type VnPayHandlers struct {
// 	*VnPayHandler
// }

// func NewVnPayHandlers(
// 	vnPay *VnPayHandler,
// ) VnPayHandlers {
// 	return VnPayHandlers{
// 		VnPayHandler: vnPay,
// 	}
// }
