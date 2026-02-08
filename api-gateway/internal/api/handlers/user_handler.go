package handlers

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/user"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
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
func (h *UserHandler) GetMe(c *gin.Context) {
	userID, exists := c.Get("userID")
	if !exists {
		c.Error(errorcode.ErrMissingTokenInfo)
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.userUC.GetById(ctx, userID.(uuid.UUID))
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
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
func (h *UserHandler) UpdateMe(c *gin.Context) {
	var req user.UpdateMeRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	userID, exists := c.Get("userID")
	if !exists {
		c.Error(errorcode.ErrMissingTokenInfo)
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.userUC.Update(ctx, userID.(uuid.UUID), &user.UpdateUserRequest{
		Email:       req.Email,
		Phone:       req.Phone,
		Address:     req.Address,
		FirstName:   req.FirstName,
		LastName:    req.LastName,
		Gender:      req.Gender,
		DateOfBirth: req.DateOfBirth,
	})
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
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
func (h *UserHandler) GetAll(c *gin.Context) {
	var req user.GetAllUsersRequest
	if err := c.ShouldBindQuery(&req); err != nil {
		c.Error(err)
		return
	}

	ctx, err := grpcutils.BuildAuthContext(c)
	if err != nil {
		c.Error(err)
		return
	}

	res, err := h.userUC.GetAll(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
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
func (h *UserHandler) GetById(c *gin.Context) {
	id := c.Param("id")
	userId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.userUC.GetById(ctx, userId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)

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
func (h *UserHandler) Create(c *gin.Context) {
	var req user.CreateUserRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	res, err := h.userUC.Create(ctx, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
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
func (h *UserHandler) UpdateRole(c *gin.Context) {
	var req user.UpdateUserRoleRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	id := c.Param("id")
	userId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.userUC.Update(ctx, userId, &user.UpdateUserRequest{
		RoleSlug: req.RoleSlug,
	})
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}
