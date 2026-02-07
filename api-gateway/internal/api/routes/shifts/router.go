package shifts

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"

	"github.com/gin-gonic/gin"
)

type ShiftRouter struct{}

func (a *ShiftRouter) NewShiftRouter(cfg *config.Config,
	r *gin.RouterGroup,
	shiftHandler *handlers.ShiftHandler,
) {
	// shiftApi := r.Group("/shifts")
	// {
	// }

	// accessTokenSecret := cfg.JwtSecret
	// privateApi := shiftApi.Group("")
	// privateApi.Use(middleware.ValidateToken([]byte(accessTokenSecret)))
	// {
	// }
}
