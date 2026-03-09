package identity

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/user"
	"api-gateway/pkg/utils/contextutils"
	"api-gateway/pkg/utils/validation"
	"io"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type UserHandler struct {
	userUC identity.IUserService
}

func NewUserHandler(userUC identity.IUserService) *UserHandler {
	return &UserHandler{userUC: userUC}
}

// --- Profile Section ---

// GetMe
// @Summary      Get My Profile
// @Description  Retrieve the profile information of the currently logged-in user.
// @Tags         Profile
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Success      200  {object}  map[string]interface{} "User profile details"
// @Failure      401  {object}  map[string]string      "Unauthorized"
// @Router       /users/me [get]
func (h *UserHandler) GetMe(c *gin.Context) error {
	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return errorcode.ErrMissingTokenInfo
	}

	ctx := c.Request.Context()
	res, err := h.userUC.GetById(ctx, userId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// UpdateMe
// @Summary      Update My Profile
// @Description  Update personal information (Email, Phone, Address, etc.) for the logged-in user.
// @Tags         Profile
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     user.UpdateMeRequest  true  "Updated profile details"
// @Success      200     {object} map[string]interface{}      "Updated profile"
// @Failure      400     {object} map[string]string           "Validation error"
// @Failure      401     {object} map[string]string           "Unauthorized"
// @Router       /users/me [patch]
func (h *UserHandler) UpdateMe(c *gin.Context) error {
	var req user.UpdateMeRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	userId, err := contextutils.GetUserId(c)
	if err != nil {
		return errorcode.ErrMissingTokenInfo
	}

	ctx := c.Request.Context()
	res, err := h.userUC.Update(ctx, userId, &user.UpdateUserRequest{
		// Email:       req.Email,
		Phone:       req.Phone,
		Address:     req.Address,
		FirstName:   req.FirstName,
		LastName:    req.LastName,
		Gender:      req.Gender,
		DateOfBirth: req.DateOfBirth,
	})
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// UploadMyAvatar
// @Summary      Upload My Avatar
// @Description  Upload my avatar. Supports JPG, PNG, and WebP formats. Max file size is 10MB.
// @Tags         Profile
// @Accept       multipart/form-data
// @Produce      json
// @Security     BearerAuth
// @Param        image      formData  file    true   "Image file to upload"
// @Success      200        {object}  map[string]string
// @Failure      401  		{object}  map[string]string "Unauthorized"
// @Failure      413        {object}  map[string]string "Request entity too large"
// @Failure      500        {object}  map[string]string "Internal server error"
// @Router       /users/me/avatar [put]
func (h *UserHandler) UploadMyAvatar(c *gin.Context) error {
	// 1. Lấy file từ form-data (key là "image")
	file, _, err := c.Request.FormFile("image")
	if err != nil {
		return errorcode.ErrImageRequired
	}
	defer file.Close()

	// 2. Đọc file vào byte slice
	fileBytes, err := io.ReadAll(file)
	if err != nil {
		return err
	}

	// 3. Validation: Kiểm tra xem có thực sự là ảnh không (Magic Bytes)
	contentType := http.DetectContentType(fileBytes)
	allowedTypes := map[string]bool{
		"image/jpeg": true,
		"image/png":  true,
		"image/webp": true,
	}
	if !allowedTypes[contentType] {
		return errorcode.ErrInvalidImageFormat
	}

	ctx := c.Request.Context()
	if err := h.userUC.UploadMyAvatar(ctx, fileBytes); err != nil {
		return err
	}

	// Trả kết quả
	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// DeleteMyAvatar
// @Summary      Delete My Avatar
// @Description  Delete my avatar.
// @Tags         Profile
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Success      200        {object}  map[string]string
// @Failure      401  		{object}  map[string]string "Unauthorized"
// @Failure      500        {object}  map[string]string "Internal server error"
// @Router       /users/me/avatar [delete]
func (h *UserHandler) DeleteMyAvatar(c *gin.Context) error {
	ctx := c.Request.Context()
	if err := h.userUC.DeleteMyAvatar(ctx); err != nil {
		return err
	}

	// Trả kết quả
	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// --- Admin / User Management Section ---

// GetAll
// @Summary      Get All Users
// @Description  Retrieve a list of users with optional filtering (Admin).
// @Tags         Users
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        query  query     user.GetAllUsersRequest  false  "Filter options"
// @Success      200    {object}  map[string]interface{}   "List of users"
// @Failure      500    {object}  map[string]string        "Internal server error"
// @Router       /users [get]
func (h *UserHandler) GetAll(c *gin.Context) error {
	var req user.GetAllUsersRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		return err
	}

	ctx := c.Request.Context()
	res, err := h.userUC.GetAll(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// GetById
// @Summary      Get User by ID
// @Description  Retrieve details of a specific user by their ID.
// @Tags         Users
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string  true  "User UUID"
// @Success      200  {object}  map[string]interface{} "User details"
// @Failure      400  {object}  map[string]string      "Invalid ID format"
// @Failure      404  {object}  map[string]string      "User not found"
// @Router       /users/{id} [get]
func (h *UserHandler) GetById(c *gin.Context) error {
	id := c.Param("id")
	userId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	res, err := h.userUC.GetById(ctx, userId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Create
// @Summary      Create New User (Admin)
// @Description  Manually create a new user (e.g., for employees or admin accounts).
// @Tags         Users
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     user.CreateUserRequest  true  "User creation details"
// @Success      200     {object} map[string]interface{}        "Created user details"
// @Failure      400     {object} map[string]string             "Validation error"
// @Router       /users [post]
func (h *UserHandler) Create(c *gin.Context) error {
	var req user.CreateUserRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	res, err := h.userUC.Create(ctx, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// Update
// @Summary      Update User
// @Description  Update a specific user info.
// @Tags         Users
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                    true  "User UUID"
// @Param        request  body     user.UpdateUserRequest 	 true "New User Info"
// @Success      200      {object} map[string]interface{}    "Updated user details"
// @Failure      400      {object} map[string]string         "Validation error"
// @Router       /users/{id} [patch]
func (h *UserHandler) Update(c *gin.Context) error {
	var req user.UpdateUserRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	id := c.Param("id")
	userId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	res, err := h.userUC.Update(ctx, userId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// UpdateRole
// @Summary      Update User Role
// @Description  Change the role (permission level) of a specific user.
// @Tags         Users
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                    true  "User UUID"
// @Param        request  body     user.UpdateUserRoleRequest true "New Role Slug"
// @Success      200      {object} map[string]interface{}    "Updated user details"
// @Failure      400      {object} map[string]string         "Validation error"
// @Router       /users/{id}/role [patch]
func (h *UserHandler) UpdateRole(c *gin.Context) error {
	var req user.UpdateUserRoleRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	id := c.Param("id")
	userId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	res, err := h.userUC.Update(ctx, userId, &user.UpdateUserRequest{
		RoleSlug: req.RoleSlug,
	})
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}
