package role

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

type RoleRouter struct{}

func (u *RoleRouter) NewRoleRouter(cfg *config.Config,
	r *gin.RouterGroup,
	roleHandler *handlers.RoleHandler,
	authMW *middleware.AuthMiddleware,
) {
	roleApi := r.Group("/roles")
	{
		roleApi.GET("", roleHandler.GetAll)
	}

	accessTokenSecret := cfg.JwtSecret
	privateApi := roleApi.Group("")
	privateApi.Use(authMW.ValidateToken([]byte(accessTokenSecret)))
	{
		privateApi.POST("",
			authMW.PermissionCheck("role.create"),
			roleHandler.Create)
		privateApi.PATCH("/:id",
			authMW.PermissionCheck("role.update"),
			roleHandler.Update)
	}

	rolePermApi := privateApi.Group("/:id/permissions")
	{
		rolePermApi.GET("",
			authMW.PermissionCheck("role.read_permission"),
			roleHandler.GetPermissionsByRoleId)
		rolePermApi.PUT("",
			authMW.PermissionCheck("role.update_permission"),
			roleHandler.UpdateRolePermissions)
	}
}
