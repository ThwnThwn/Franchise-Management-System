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
	}

	registerApi := authApi.Group("/register")
	{
		registerApi.POST("", authHander.Register)
		registerApi.POST("/confirm-otp", authHander.ConfirmRegisterOtp)
		registerApi.POST("/resend-otp", authHander.ReSendRegisterOtp)
	}

	forgotPasswordApi := authApi.Group("/forgot-password")
	{
		forgotPasswordApi.POST("", authHander.ForgotPassword)
		forgotPasswordApi.POST("/confirm-otp", authHander.ConfirmForgotPasswordOtp)
		forgotPasswordApi.POST("/new-password", authHander.NewPassword)
		forgotPasswordApi.POST("/resend-otp", authHander.ReSendForgotPasswordOtp)
	}

	accessTokenSecret := cfg.JwtSecret
	privateApi := authApi.Group("")
	privateApi.Use(middleware.ValidateToken([]byte(accessTokenSecret)))
	{
		privateApi.POST("/logout", authHander.Logout)
		privateApi.POST("/change-password", authHander.ChangePassword)
	}
}
