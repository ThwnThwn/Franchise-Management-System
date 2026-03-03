package identity

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

// Login
// @Summary      Login
// @Description  Enter username/email and password to get access token
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Param        request body auth.LoginRequest true "Login Request Body"
// @Success      200  {object}  map[string]interface{}
// @Failure      400  {object}  map[string]string
// @Router       /auth/login [post]
func (h *AuthHandler) Login(c *gin.Context) error {
	var req auth.LoginRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.authGW.Login(ctx, &req)
	if err != nil {
		return err
	}

	c.SetCookie("refresh_token", res.RefreshToken,
		60*60*24*global.Config.JwtRefreshTokenExpDays, "/", "", false, true)

	c.JSON(http.StatusOK, gin.H{
		"accessToken": res.AccessToken,
		"user":        res.User,
	})
	return nil
}

// Logout
// @Summary      Logout user
// @Description  Invalidates the current access token and clears the refresh token cookie.
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Success      200  {object}  map[string]interface{}  "Returns success status"
// @Failure      401  {object}  map[string]string       "Unauthorized or invalid token"
// @Router       /auth/logout [post]
func (h *AuthHandler) Logout(c *gin.Context) error {
	accessToken, exists := c.Get("accessToken")
	if !exists {
		return errorcode.ErrInvalidAccessToken
	}
	refreshToken, err := c.Cookie("refresh_token")
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	res, err := h.authGW.Logout(ctx, &auth.LogoutRequest{
		AccessToken:  accessToken.(string),
		RefreshToken: refreshToken,
	})
	if err != nil {
		return err
	}

	c.SetCookie("refresh_token", "", -1, "/", "", false, true)
	c.JSON(http.StatusOK, gin.H{
		"success": res,
	})
	return nil
}

// RefreshToken
// @Summary      Refresh Access Token
// @Description  Obtains a new access token using the valid refresh token stored in the HttpOnly cookie.
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Success      200  {object}  map[string]interface{}  "Returns new Access Token"
// @Failure      400  {object}  map[string]string       "Missing cookie"
// @Failure      401  {object}  map[string]string       "Invalid refresh token"
// @Router       /auth/refresh-token [post]
func (h *AuthHandler) RefreshToken(c *gin.Context) error {
	refreshToken, err := c.Cookie("refresh_token")
	if err != nil {
		return err
	}

	ctx := c.Request.Context()
	res, err := h.authGW.RefreshToken(ctx, &auth.RefreshTokenRequest{
		RefreshToken: refreshToken,
	})
	if err != nil {
		return err
	}

	c.SetCookie("refresh_token", res.RefreshToken,
		60*60*24*global.Config.JwtRefreshTokenExpDays, "/", "", false, true)

	c.JSON(http.StatusOK, gin.H{
		"accessToken": res.AccessToken,
	})
	return nil
}

// Register

// Register
// @Summary      Register a new account
// @Description  Create a new user account. After successful registration, an OTP will be sent to the email for verification.
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Param        request body auth.RegisterRequest true "Registration details"
// @Success      200  {object}  map[string]interface{} "Returns success message"
// @Failure      400  {object}  map[string]string      "Validation error"
// @Failure      500  {object}  map[string]string      "Internal server error"
// @Router       /auth/register [post]
func (h *AuthHandler) Register(c *gin.Context) error {
	var req auth.RegisterRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
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
		return err
	}

	c.JSON(http.StatusOK, gin.H{
		"message": res,
	})
	return nil
}

// ConfirmRegisterOtp
// @Summary      Verify Registration OTP
// @Description  Verify the OTP sent to email to activate the new account.
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Param        request body auth.ConfirmOtpRequest true "Email and OTP"
// @Success      200  {object}  map[string]interface{} "Returns success status and message"
// @Failure      400  {object}  map[string]string      "Invalid OTP or validation error"
// @Router       /auth/register/confirm-otp [post]
func (h *AuthHandler) ConfirmRegisterOtp(c *gin.Context) error {
	var req auth.ConfirmOtpRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	success, msg, err := h.authGW.ConfirmRegisterOtp(ctx, &auth.ConfirmOtpRequest{
		Email: req.Email,
		Otp:   req.Otp,
	})

	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{
		"success": success,
		"message": msg,
	})
	return nil
}

// ReSendRegisterOtp
// @Summary      Resend Registration OTP
// @Description  Resend the activation OTP if the previous one expired or was not received.
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Param        request body auth.SendOtpRequest true "Email address"
// @Success      200  {object}  map[string]interface{} "Returns success message"
// @Failure      400  {object}  map[string]string      "Validation error"
// @Router       /auth/register/resend-otp [post]
func (h *AuthHandler) ReSendRegisterOtp(c *gin.Context) error {
	var req auth.SendOtpRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	msg, err := h.authGW.ReSendOtp(ctx, req.Email, jwttype.Register)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{
		"message": msg,
	})
	return nil
}

// Forgot password

// ForgotPassword
// @Summary      Request Password Reset
// @Description  Initiates the password reset process by sending an OTP to the provided email address.
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Param        request body auth.SendOtpRequest true "Email address"
// @Success      200  {object}  map[string]interface{} "Returns success message"
// @Failure      400  {object}  map[string]string      "Validation error"
// @Failure      500  {object}  map[string]string      "Internal server error"
// @Router       /auth/forgot-password [post]
func (h *AuthHandler) ForgotPassword(c *gin.Context) error {
	var req auth.SendOtpRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	msg, err := h.authGW.ForgotPassword(ctx, req.Email)

	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{
		"message": msg,
	})
	return nil
}

// ConfirmForgotPasswordOtp
// @Summary      Verify Password Reset OTP
// @Description  Verifies the OTP sent to the email. If successful, a 'forgot_password_token' HttpOnly cookie will be set, which is required for setting the new password.
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Param        request body auth.ConfirmOtpRequest true "Email and OTP"
// @Success      200  {object}  map[string]interface{} "Returns success message and sets cookie"
// @Failure      400  {object}  map[string]string      "Invalid OTP or validation error"
// @Router       /auth/forgot-password/confirm-otp [post]
func (h *AuthHandler) ConfirmForgotPasswordOtp(c *gin.Context) error {
	var req auth.ConfirmOtpRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	token, err := h.authGW.ConfirmForgotPasswordOtp(ctx, &auth.ConfirmOtpRequest{
		Email: req.Email,
		Otp:   req.Otp,
	})

	if err != nil {
		return err
	}

	c.SetCookie("forgot_password_token", token,
		60*global.Config.JwtAccessTokenExpirationMinutes, "/", "", false, true)

	c.JSON(http.StatusOK, gin.H{
		"message": "Xác thực OTP thành công",
	})
	return nil
}

// NewPassword
// @Summary      Set New Password
// @Description  Finalizes the password reset process. This endpoint requires the 'forgot_password_token' cookie obtained from the OTP verification step.
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Param        request body auth.NewPasswordRequest true "New Password"
// @Success      200  {object}  map[string]interface{} "Returns success status"
// @Failure      400  {object}  map[string]string      "Missing cookie or validation error"
// @Failure      401  {object}  map[string]string      "Invalid or expired token"
// @Router       /auth/forgot-password/new-password [post]
func (h *AuthHandler) NewPassword(c *gin.Context) error {
	var req auth.NewPasswordRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	token, err := c.Cookie("forgot_password_token")
	if err != nil {
		return err
	}

	claims, err := jwtutils.ValidateToken([]byte(global.Config.JwtSecret), token, jwttype.ForgotPassword)
	if err != nil {
		return err
	}

	md := metadata.Pairs(
		"x-email", claims.Subject,
	)
	ctx := metadata.NewOutgoingContext(c.Request.Context(), md)
	success, msg, err := h.authGW.NewPassword(ctx, req.NewPassword)
	if err != nil {
		return err
	}

	c.SetCookie("forgot_password_token", "", -1, "/", "", false, true)

	c.JSON(http.StatusOK, gin.H{
		"success": success,
		"message": msg,
	})
	return nil
}

// ReSendForgotPasswordOtp
// @Summary      Resend Password Reset OTP
// @Description  Resends the OTP for password reset if the previous one expired.
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Param        request body auth.SendOtpRequest true "Email address"
// @Success      200  {object}  map[string]interface{} "Returns success message"
// @Failure      400  {object}  map[string]string      "Validation error"
// @Router       /auth/forgot-password/resend-otp [post]
func (h *AuthHandler) ReSendForgotPasswordOtp(c *gin.Context) error {
	var req auth.SendOtpRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	msg, err := h.authGW.ReSendOtp(ctx, req.Email, jwttype.ForgotPassword)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{
		"message": msg,
	})
	return nil
}

// Change password

// ChangePassword
// @Summary      Change Password
// @Description  Allows the logged-in user to change their password. Requires a valid Access Token.
// @Tags         Auth
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body user.ChangePasswordRequest true "Old and New Password"
// @Success      200  {object}  map[string]interface{} "Returns success status"
// @Failure      400  {object}  map[string]string      "Validation error"
// @Failure      401  {object}  map[string]string      "Unauthorized or missing token"
// @Router       /auth/change-password [post]
func (h *AuthHandler) ChangePassword(c *gin.Context) error {
	var req user.ChangePasswordRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	accessToken := c.GetString("accessToken")
	if accessToken == "" {
		return errorcode.ErrMissingTokenInfo
	}

	ctx := c.Request.Context()
	res, err := h.userGW.ChangePassword(ctx, accessToken, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}
