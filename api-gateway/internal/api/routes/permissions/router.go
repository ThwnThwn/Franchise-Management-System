package permissions

import (
	"github.com/gin-gonic/gin"
)

func (p *PermissionRouter) Init(r *gin.RouterGroup) {
	permApi := r.Group("/permissions")

	privateApi := permApi.Group("")
	privateApi.Use(p.authMW.ValidateToken([]byte(p.cfg.JwtSecret)))
	{
		privateApi.GET("", p.authMW.PermissionCheck("permission.read"), p.permHl.GetAllPermissionsGrouped)
		privateApi.PATCH("/:id", p.authMW.PermissionCheck("permission.update"), p.permHl.UpdatePermission)
	}
}
