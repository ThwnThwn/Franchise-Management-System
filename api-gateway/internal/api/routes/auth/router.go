package auth

import (
	"api-gateway/internal/api"

	"github.com/gin-gonic/gin"
)

func (a *AuthRouter) Init(r *gin.RouterGroup) {
	authApi := r.Group("/auth")
	{
		authApi.POST("/login", api.WrapHandler(a.authHl.Login))
		authApi.POST("/refresh-token", api.WrapHandler(a.authHl.RefreshToken))
	}

	registerApi := authApi.Group("/register")
	{
		registerApi.POST("", api.WrapHandler(a.authHl.Register))
		registerApi.POST("/confirm-otp", api.WrapHandler(a.authHl.ConfirmRegisterOtp))
		registerApi.POST("/resend-otp", api.WrapHandler(a.authHl.ReSendRegisterOtp))
	}

	forgotPasswordApi := authApi.Group("/forgot-password")
	{
		forgotPasswordApi.POST("", api.WrapHandler(a.authHl.ForgotPassword))
		forgotPasswordApi.POST("/confirm-otp", api.WrapHandler(a.authHl.ConfirmForgotPasswordOtp))
		forgotPasswordApi.POST("/new-password", api.WrapHandler(a.authHl.NewPassword))
		forgotPasswordApi.POST("/resend-otp", api.WrapHandler(a.authHl.ReSendForgotPasswordOtp))
	}

	privateApi := authApi.Group("")
	privateApi.Use(a.authMW.ValidateToken([]byte(a.cfg.JwtSecret)))
	{
		privateApi.POST("/logout", api.WrapHandler(a.authHl.Logout))
		privateApi.POST("/change-password", api.WrapHandler(a.authHl.ChangePassword))
	}
}
