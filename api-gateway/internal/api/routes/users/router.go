package users

import (
	"github.com/gin-gonic/gin"
)

func (u *UserRouter) Init(r *gin.RouterGroup) {
	userApi := r.Group("/users")

	privateApi := userApi.Group("")
	privateApi.Use(u.authMW.ValidateToken([]byte(u.cfg.JwtSecret)))
	{
		privateApi.GET("/me", u.userHl.GetMe)
		privateApi.PATCH("/me", u.userHl.UpdateMe)
		privateApi.PUT("/me/avatar", u.userHl.UploadMyAvatar)
		privateApi.DELETE("/me/avatar", u.userHl.DeleteMyAvatar)
	}

	{
		// Admin lấy danh sách user
		privateApi.GET("", u.authMW.PermissionCheck("user.read"), u.userHl.GetAll)
		// Admin lấy thông tin 1 user
		privateApi.GET("/:id", u.authMW.PermissionCheck("user.read"), u.userHl.GetById)
		// Admin tạo 1 user
		privateApi.POST("", u.authMW.PermissionCheck("user.create"), u.userHl.Create)
		// Admin update role cho 1 user
		privateApi.PATCH("/:id/role", u.authMW.PermissionCheck("user.update"), u.userHl.UpdateRole)
	}
}
