package handlers

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/permission"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type PermHandler struct {
	rolePermGW identity.IRolePermissionService
}

func NewPermHandler(rolePermGW identity.IRolePermissionService) *PermHandler {
	return &PermHandler{
		rolePermGW: rolePermGW,
	}
}

func (h *PermHandler) GetAllPermissionsGrouped(c *gin.Context) {
	ctx := c.Request.Context()
	res, err := h.rolePermGW.GetAllPermissionsGrouped(ctx)
	if err != nil {
		c.Error(err)
		return
	}
	c.JSON(http.StatusOK, res)
}

func (h *PermHandler) UpdatePermission(c *gin.Context) {
	id := c.Param("id")
	permId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req permission.UpdatePermissionInfoRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err = h.rolePermGW.UpdatePermissionInfo(ctx, permId, &req)
	if err != nil {
		c.Error(err)
		return
	}
	c.JSON(http.StatusOK, gin.H{})
}
