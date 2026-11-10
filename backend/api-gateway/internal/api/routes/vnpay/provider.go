package vnpay

import "api-gateway/internal/api/handlers/vnpay"

type VnPayRouter struct {
	vnpHl *vnpay.VnPayHandler
}

func NewRouter(hl *vnpay.VnPayHandler) *VnPayRouter {
	return &VnPayRouter{vnpHl: hl}
}
