package roles

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (rR *RoleRouter) Init(r *gin.RouterGroup) {
	roleApi := r.Group("/roles")
	{
		roleApi.GET("", api.WrapHandler(rR.roleHl.GetAll))
	}

	privateApi := roleApi.Group("")
	privateApi.Use(rR.authMW.ValidateToken([]byte(rR.cfg.JwtSecret)))
	{
		privateApi.POST("",
			rR.authMW.PermissionCheck("role.create"),
			api.WrapHandler(rR.roleHl.Create))
		privateApi.PATCH("/:id",
			rR.authMW.PermissionCheck("role.update"),
			api.WrapHandler(rR.roleHl.Update))
	}

	rolePermApi := privateApi.Group("/:id/permissions")
	{
		rolePermApi.GET("",
			rR.authMW.PermissionCheck("role.permission.read"),
			api.WrapHandler(rR.roleHl.GetPermissionsByRoleId))
		rolePermApi.PUT("",
			rR.authMW.PermissionCheck("role.permission.update"),
			api.WrapHandler(rR.roleHl.UpdateRolePermissions))
	}
}
