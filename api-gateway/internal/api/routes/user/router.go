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
) {
	userApi := r.Group("/users")

	accessTokenSecret := cfg.JwtSecret
	privateApi := userApi.Group("/")
	privateApi.Use(middleware.ValidateToken([]byte(accessTokenSecret)))
	{
		privateApi.POST("/me", userHander.GetMe)
	}
}
