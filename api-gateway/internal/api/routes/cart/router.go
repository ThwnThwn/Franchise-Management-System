package cart

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
	"api-gateway/internal/application/abstractions"

	"github.com/gin-gonic/gin"
)

type CartRouter struct{}

func (a *CartRouter) NewCartRouter(cfg *config.Config,
	r *gin.RouterGroup,
	cartHander *handlers.CartHandler,
	blackListUC abstractions.IBlacklistUseCase,
) {
	cartApi := r.Group("/carts")

	accessTokenSecret := cfg.JwtSecret
	privateApi := cartApi.Group("")
	privateApi.Use(middleware.ValidateToken(blackListUC, []byte(accessTokenSecret)))
	{
		privateApi.GET("", cartHander.GetCartItems)
		privateApi.POST("", cartHander.AddToCart)
		privateApi.PUT("", cartHander.UpdateCartItem)
		privateApi.DELETE("", cartHander.DeleteListCartItem)
		privateApi.DELETE("/clear", cartHander.ClearCart)
	}
}
