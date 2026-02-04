package user

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

type UserRouter struct{}

func (u *UserRouter) NewUserRouter(cfg *config.Config,
	r *gin.RouterGroup,
	userHander *handlers.UserHandler,
	shiftHandler *handlers.ShiftHandler,
	authMW *middleware.AuthMiddleware,
) {
	userApi := r.Group("/users")

	accessTokenSecret := cfg.JwtSecret
	privateApi := userApi.Group("")
	privateApi.Use(authMW.ValidateToken([]byte(accessTokenSecret)))
	{
		privateApi.GET("/me", userHander.GetMe)
		privateApi.PATCH("/me", userHander.UpdateMe)
	}

	// staffApi := privateApi.Group("")
	// staffApi.Use(middleware.RequireRoles(string(roleslug.Staff)))
	{
		// Staff láy thông tin ca làm hiện tại
		privateApi.GET("/me/current-shift", authMW.PermissionCheck("me.current_shift"), shiftHandler.GetUserCurrentShift)
	}

	// adminApi := privateApi.Group("")
	// adminApi.Use(middleware.RequireRoles(string(roleslug.Admin)))
	{
		// Admin lấy danh sách user
		privateApi.GET("", authMW.PermissionCheck("user.read"), userHander.GetAll)
		// Admin lấy thông tin 1 user
		privateApi.GET("/:id", authMW.PermissionCheck("user.read"), userHander.GetById)
		// Admin tạo 1 user
		privateApi.POST("", authMW.PermissionCheck("user.create"), userHander.Create)
		// Admin update role cho 1 user
		privateApi.PATCH("/:id/role", authMW.PermissionCheck("user.update"), userHander.UpdateRole)
	}
}
