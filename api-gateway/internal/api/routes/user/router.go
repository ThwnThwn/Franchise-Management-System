package user

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
	"api-gateway/internal/domain/constants/roleslug"

	"github.com/gin-gonic/gin"
)

type UserRouter struct{}

func (u *UserRouter) NewUserRouter(cfg *config.Config,
	r *gin.RouterGroup,
	userHander *handlers.UserHandler,
	shiftHandler *handlers.ShiftHandler,
) {
	userApi := r.Group("/users")

	accessTokenSecret := cfg.JwtSecret
	privateApi := userApi.Group("")
	privateApi.Use(middleware.ValidateToken([]byte(accessTokenSecret)))
	{
		privateApi.GET("/me", userHander.GetMe)
		privateApi.PATCH("/me", userHander.UpdateMe)
	}

	staffApi := privateApi.Group("")
	staffApi.Use(middleware.RequireRoles(string(roleslug.Staff)))
	{
		// Staff láy thông tin ca làm hiện tại
		staffApi.GET("/me/current-shift", shiftHandler.GetUserCurrentShift)
	}

	adminApi := privateApi.Group("")
	adminApi.Use(middleware.RequireRoles(string(roleslug.Admin)))
	{
		// Admin update role cho 1 user
		adminApi.PATCH("/:id/role", userHander.UpdateRole)
	}
}
