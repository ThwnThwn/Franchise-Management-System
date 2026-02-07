package roles

import (
	"github.com/gin-gonic/gin"
)

func (rR *RoleRouter) Init(r *gin.RouterGroup) {
	roleApi := r.Group("/roles")
	{
		roleApi.GET("", rR.roleHl.GetAll)
	}

	privateApi := roleApi.Group("")
	privateApi.Use(rR.authMW.ValidateToken([]byte(rR.cfg.JwtSecret)))
	{
		privateApi.POST("",
			rR.authMW.PermissionCheck("role.create"),
			rR.roleHl.Create)
		privateApi.PATCH("/:id",
			rR.authMW.PermissionCheck("role.update"),
			rR.roleHl.Update)
	}

	rolePermApi := privateApi.Group("/:id/permissions")
	{
		rolePermApi.GET("",
			rR.authMW.PermissionCheck("role.read_permission"),
			rR.roleHl.GetPermissionsByRoleId)
		rolePermApi.PUT("",
			rR.authMW.PermissionCheck("role.update_permission"),
			rR.roleHl.UpdateRolePermissions)
	}
}
