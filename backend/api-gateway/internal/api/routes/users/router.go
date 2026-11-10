package users

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (u *UserRouter) Init(r *gin.RouterGroup) {
	userApi := r.Group("/users")

	privateApi := userApi.Group("")
	privateApi.Use(u.authMW.ValidateToken([]byte(u.cfg.JwtSecret)))
	{
		privateApi.GET("/me", api.WrapHandler(u.userHl.GetMe))
		privateApi.PATCH("/me", api.WrapHandler(u.userHl.UpdateMe))
		privateApi.PUT("/me/avatar", api.WrapHandler(u.userHl.UploadMyAvatar))
		privateApi.DELETE("/me/avatar", api.WrapHandler(u.userHl.DeleteMyAvatar))
	}

	{
		// lấy danh sách user
		privateApi.GET("", u.authMW.PermissionCheck("user.read"),
			api.WrapHandler(u.userHl.GetAll))

		// lấy thông tin 1 user
		privateApi.GET("/:id", u.authMW.PermissionCheck("user.read"),
			api.WrapHandler(u.userHl.GetById))

		// tạo 1 user
		privateApi.POST("", u.authMW.PermissionCheck("user.create"),
			api.WrapHandler(u.userHl.Create))

		// update 1 user
		privateApi.PATCH("/:id", u.authMW.PermissionCheck("user.update"),
			api.WrapHandler(u.userHl.Update))

		// update role cho 1 user
		privateApi.PATCH("/:id/role", u.authMW.PermissionCheck("user.update"),
			api.WrapHandler(u.userHl.UpdateRole))
	}
}
