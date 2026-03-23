package vnpay

import (
	"github.com/gin-gonic/gin"
)

func (r *VnPayRouter) Init(router *gin.RouterGroup) {
	// Endpoint: /api/vnpay/ipn (Public - No Auth)
	vnpay := router.Group("/vnpay")
	{
		vnpay.GET("/ipn", r.vnpHl.HandleIPN)
	}
}
