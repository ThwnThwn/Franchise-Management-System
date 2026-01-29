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
) {
	userApi := r.Group("/users")

	accessTokenSecret := cfg.JwtSecret
	privateApi := userApi.Group("")
	privateApi.Use(middleware.ValidateToken([]byte(accessTokenSecret)))
	{
		privateApi.GET("/me", userHander.GetMe)
		privateApi.PATCH("/me", userHander.UpdateMe)
	}

	adminApi := privateApi.Group("")
	adminApi.Use(middleware.RequireRoles(string(roleslug.Admin)))
	{
		adminApi.PATCH("/:id/role", userHander.UpdateRole)
	}
}
