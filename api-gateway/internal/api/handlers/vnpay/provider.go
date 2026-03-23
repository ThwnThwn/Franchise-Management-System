package vnpay

import "github.com/google/wire"

var ProviderSet = wire.NewSet(
	NewVnPayHandler,
)
