package auth

import (
	"github.com/gin-gonic/gin"
)

func (a *AuthRouter) Init(r *gin.RouterGroup) {
	authApi := r.Group("/auth")
	{
		authApi.POST("/login", a.authHl.Login)
		authApi.POST("/refresh-token", a.authHl.RefreshToken)
	}

	registerApi := authApi.Group("/register")
	{
		registerApi.POST("", a.authHl.Register)
		registerApi.POST("/confirm-otp", a.authHl.ConfirmRegisterOtp)
		registerApi.POST("/resend-otp", a.authHl.ReSendRegisterOtp)
	}

	forgotPasswordApi := authApi.Group("/forgot-password")
	{
		forgotPasswordApi.POST("", a.authHl.ForgotPassword)
		forgotPasswordApi.POST("/confirm-otp", a.authHl.ConfirmForgotPasswordOtp)
		forgotPasswordApi.POST("/new-password", a.authHl.NewPassword)
		forgotPasswordApi.POST("/resend-otp", a.authHl.ReSendForgotPasswordOtp)
	}

	privateApi := authApi.Group("")
	privateApi.Use(a.authMW.ValidateToken([]byte(a.cfg.JwtSecret)))
	{
		privateApi.POST("/logout", a.authHl.Logout)
		privateApi.POST("/change-password", a.authHl.ChangePassword)
	}
}
