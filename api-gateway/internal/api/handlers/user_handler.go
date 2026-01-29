package handlers

import (
	"api-gateway/internal/application/abstractions"
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/domain/models/user"
	"api-gateway/pkg/utils/grpcutils"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type UserHandler struct {
	userUC abstractions.IUserUseCase
}

func NewUserHandler(userUC abstractions.IUserUseCase) *UserHandler {
	return &UserHandler{userUC: userUC}
}

// Profile
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
