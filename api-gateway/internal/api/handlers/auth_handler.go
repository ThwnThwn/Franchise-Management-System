package handlers

import (
	"api-gateway/global"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/constants/jwttype"
	"api-gateway/internal/domain/models/auth"
	"api-gateway/internal/domain/models/user"
	"api-gateway/pkg/utils/jwtutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"google.golang.org/grpc/metadata"
)

type AuthHandler struct {
	authGW identity.IAuthService
	userGW identity.IUserService
}

func NewAuthHandler(
	authGW identity.IAuthService,
	userGW identity.IUserService,
) *AuthHandler {
	return &AuthHandler{
		authGW: authGW,
		userGW: userGW,
	}
}

func (h *AuthHandler) Login(c *gin.Context) {
	var req auth.LoginRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	res, err := h.authGW.Login(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.SetCookie("refresh_token", res.RefreshToken,
		60*60*24*global.Config.JwtRefreshTokenExpDays, "/", "", false, true)

	c.JSON(http.StatusOK, gin.H{
		"accessToken": res.AccessToken,
		"user":        res.User,
	})
}

func (h *AuthHandler) Logout(c *gin.Context) {
	accessToken, exists := c.Get("accessToken")
	if !exists {
		c.Error(errorcode.ErrInvalidAccessToken)
		return
	}
	refreshToken, err := c.Cookie("refresh_token")
	if err != nil {
		c.Error(err)
		return
	}

	ctx := c.Request.Context()
	res, err := h.authGW.Logout(ctx, &auth.LogoutRequest{
		AccessToken:  accessToken.(string),
		RefreshToken: refreshToken,
	})
	if err != nil {
		c.Error(err)
		return
	}

	c.SetCookie("refresh_token", "", -1, "/", "", false, true)
	c.JSON(http.StatusOK, gin.H{
		"success": res,
	})
}

func (h *AuthHandler) RefreshToken(c *gin.Context) {
	refreshToken, err := c.Cookie("refresh_token")
	if err != nil {
		c.Error(err)
		return
	}

	ctx := c.Request.Context()
	res, err := h.authGW.RefreshToken(ctx, &auth.RefreshTokenRequest{
		RefreshToken: refreshToken,
	})
	if err != nil {
		c.Error(err)
		return
	}

	c.SetCookie("refresh_token", res.RefreshToken,
		60*60*24*global.Config.JwtRefreshTokenExpDays, "/", "", false, true)

	c.JSON(http.StatusOK, gin.H{
		"accessToken": res.AccessToken,
	})
}

// Register
func (h *AuthHandler) Register(c *gin.Context) {
	var req auth.RegisterRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	res, err := h.authGW.Register(ctx, &auth.RegisterRequest{
		Email:           req.Email,
		Username:        req.Username,
		FirstName:       req.FirstName,
		LastName:        req.LastName,
		Password:        req.Password,
		ConfirmPassword: req.ConfirmPassword,
		Phone:           req.Phone,
		Address:         req.Address,
		Gender:          req.Gender,
		DateOfBirth:     req.DateOfBirth,
	})

	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": res,
	})
}

func (h *AuthHandler) ConfirmRegisterOtp(c *gin.Context) {
	var req auth.ConfirmOtpRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	success, msg, err := h.authGW.ConfirmRegisterOtp(ctx, &auth.ConfirmOtpRequest{
		Email: req.Email,
		Otp:   req.Otp,
	})

	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"success": success,
		"message": msg,
	})
}

func (h *AuthHandler) ReSendRegisterOtp(c *gin.Context) {
	var req auth.SendOtpRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	msg, err := h.authGW.ReSendOtp(ctx, req.Email, jwttype.Register)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": msg,
	})
}

// Forgot password
func (h *AuthHandler) ForgotPassword(c *gin.Context) {
	var req auth.SendOtpRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	msg, err := h.authGW.ForgotPassword(ctx, req.Email)

	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": msg,
	})
}

func (h *AuthHandler) ConfirmForgotPasswordOtp(c *gin.Context) {
	var req auth.ConfirmOtpRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	token, err := h.authGW.ConfirmForgotPasswordOtp(ctx, &auth.ConfirmOtpRequest{
		Email: req.Email,
		Otp:   req.Otp,
	})

	if err != nil {
		c.Error(err)
		return
	}

	c.SetCookie("forgot_password_token", token,
		60*global.Config.JwtAccessTokenExpirationMinutes, "/", "", false, true)

	c.JSON(http.StatusOK, gin.H{
		"message": "Xác thực OTP thành công",
	})
}

func (h *AuthHandler) NewPassword(c *gin.Context) {
	var req auth.NewPasswordRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	token, err := c.Cookie("forgot_password_token")
	if err != nil {
		c.Error(err)
		return
	}

	claims, err := jwtutils.ValidateToken([]byte(global.Config.JwtSecret), token, jwttype.ForgotPassword)
	if err != nil {
		c.Error(err)
		return
	}

	md := metadata.Pairs(
		"x-email", claims.Subject,
	)
	ctx := metadata.NewOutgoingContext(c.Request.Context(), md)
	success, msg, err := h.authGW.NewPassword(ctx, req.NewPassword)
	if err != nil {
		c.Error(err)
		return
	}

	c.SetCookie("forgot_password_token", "", -1, "/", "", false, true)

	c.JSON(http.StatusOK, gin.H{
		"success": success,
		"message": msg,
	})
}

func (h *AuthHandler) ReSendForgotPasswordOtp(c *gin.Context) {
	var req auth.SendOtpRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	msg, err := h.authGW.ReSendOtp(ctx, req.Email, jwttype.ForgotPassword)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{
		"message": msg,
	})
}

// Change password
func (h *AuthHandler) ChangePassword(c *gin.Context) {
	var req user.ChangePasswordRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	accessToken := c.GetString("accessToken")
	if accessToken == "" {
		c.Error(errorcode.ErrMissingTokenInfo)
		return
	}

	ctx := c.Request.Context()
	res, err := h.userGW.ChangePassword(ctx, accessToken, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}
