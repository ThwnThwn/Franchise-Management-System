package auth

import (
	"api-gateway/config"
	"api-gateway/internal/api/handlers"
	"api-gateway/internal/api/middleware"

	"github.com/gin-gonic/gin"
)

type AuthRouter struct{}

func (a *AuthRouter) NewAuthRouter(cfg *config.Config,
	r *gin.RouterGroup,
	authHander *handlers.AuthHandler,
) {
	authApi := r.Group("/auth")
	{
		authApi.POST("/login", authHander.Login)
		authApi.POST("/refresh-token", authHander.RefreshToken)
		authApi.POST("/register", authHander.Register)
		authApi.POST("/register/confirm-otp", authHander.ConfirmOtp)
	}

	accessTokenSecret := cfg.JwtSecret
	privateApi := authApi.Group("/")
	privateApi.Use(middleware.ValidateToken([]byte(accessTokenSecret)))
	{
		privateApi.POST("/logout", authHander.Logout)
	}
}
