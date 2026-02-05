package permission

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

type PermissionRouter struct{}

func (u *PermissionRouter) NewPermissionRouter(cfg *config.Config,
	r *gin.RouterGroup,
	permHander *handlers.PermHandler,
	authMW *middleware.AuthMiddleware,
) {
	permApi := r.Group("/permissions")

	accessTokenSecret := cfg.JwtSecret
	privateApi := permApi.Group("")
	privateApi.Use(authMW.ValidateToken([]byte(accessTokenSecret)))
	{
		privateApi.GET("", authMW.PermissionCheck("permission.read"), permHander.GetAllPermissionsGrouped)
		privateApi.PATCH("/:id", authMW.PermissionCheck("permission.update"), permHander.UpdatePermission)
	}
}
