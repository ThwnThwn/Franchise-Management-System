package user

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/domain/constants/roleslug"

	"github.com/gin-gonic/gin"
)

type UserRouter struct{}

func (u *UserRouter) NewUserRouter(cfg *config.Config,
	r *gin.RouterGroup,
	userHander *handlers.UserHandler,
	shiftHandler *handlers.ShiftHandler,
	blackListUC abstractions.IBlacklistUseCase,
) {
	userApi := r.Group("/users")

	accessTokenSecret := cfg.JwtSecret
	privateApi := userApi.Group("")
	privateApi.Use(middleware.ValidateToken(blackListUC, []byte(accessTokenSecret)))
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
		// Admin lấy danh sách user
		adminApi.GET("", userHander.GetAll)
		// Admin lấy thông tin 1 user
		adminApi.GET("/:id", userHander.GetById)
		// Admin tạo 1 user
		adminApi.POST("", userHander.Create)
		// Admin update role cho 1 user
		adminApi.PATCH("/:id/role", userHander.UpdateRole)
	}
}
