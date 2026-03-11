package vnpay

import "github.com/google/wire"

type VnPayHandlers struct {
	*VnPayHandler
}

func NewVnPayHandlers(
	vnPay *VnPayHandler,
) VnPayHandlers {
	return VnPayHandlers{
		VnPayHandler: vnPay,
	}
}

var ProviderSet = wire.NewSet(
	NewVnPayHandler,

	NewVnPayHandlers,
)
