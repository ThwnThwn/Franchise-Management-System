package handlers

import (
	"api-gateway/internal/application/constants/errorcode"
	"api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/role"
	"api-gateway/pkg/utils/validation"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/google/uuid"
)

type RoleHandler struct {
	roleGW     identity.IRoleService
	rolePermGW identity.IRolePermissionService
}

func NewRoleHandler(
	roleGW identity.IRoleService,
	rolePermGW identity.IRolePermissionService,
) *RoleHandler {
	return &RoleHandler{
		roleGW:     roleGW,
		rolePermGW: rolePermGW,
	}
}

func (h *RoleHandler) GetAll(c *gin.Context) {
	ctx := c.Request.Context()
	res, err := h.roleGW.GetAll(ctx)
	if err != nil {
		c.Error(err)
		return
	}
	c.JSON(http.StatusOK, res)
}

func (h *RoleHandler) Create(c *gin.Context) {
	var req role.CreateRoleRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	if err := h.roleGW.Create(ctx, &req); err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

func (h *RoleHandler) Update(c *gin.Context) {
	id := c.Param("id")
	roleId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req role.UpdateRoleRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	if err := h.roleGW.Update(ctx, roleId, &req); err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}

// --- Role Perm ---
func (h *RoleHandler) GetPermissionsByRoleId(c *gin.Context) {
	id := c.Param("id")
	roleId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	ctx := c.Request.Context()
	res, err := h.rolePermGW.GetPermissionsByRoleId(ctx, roleId)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, res)
}

func (h *RoleHandler) UpdateRolePermissions(c *gin.Context) {
	id := c.Param("id")
	roleId, err := uuid.Parse(id)
	if err != nil {
		c.Error(errorcode.ErrInvalidId)
		return
	}

	var req role.UpdateRolePermissionsRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.Error(validation.TranslateValidationError(err))
		return
	}

	ctx := c.Request.Context()
	err = h.rolePermGW.UpdateRolePermissions(ctx, roleId, &req)
	if err != nil {
		c.Error(err)
		return
	}

	c.JSON(http.StatusOK, gin.H{})
}
