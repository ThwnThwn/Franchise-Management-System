package identity

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

// GetAllPermissionsGrouped
// @Summary      Get All Permissions (Grouped)
// @Description  Retrieve a list of all system permissions, grouped by their module or category.
// @Tags         Permissions
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Success      200  {object}  map[string]interface{} "List of permissions grouped by module"
// @Failure      500  {object}  map[string]string      "Internal server error"
// @Router       /permissions [get]
func (h *PermHandler) GetAllPermissionsGrouped(c *gin.Context) error {
	ctx := c.Request.Context()
	res, err := h.rolePermGW.GetAllPermissionsGrouped(ctx)
	if err != nil {
		return err
	}
	c.JSON(http.StatusOK, res)
	return nil
}

// UpdatePermission
// @Summary      Update Permission
// @Description  Update details (e.g., description, name) of a specific permission by its ID.
// @Tags         Permissions
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                                  true  "Permission UUID"
// @Param        request  body     permission.UpdatePermissionInfoRequest  true  "Updated details"
// @Success      200      {object} map[string]interface{}                  "Success"
// @Failure      400      {object} map[string]string                       "Validation error or Invalid ID"
// @Router       /permissions/{id} [patch]
func (h *PermHandler) UpdatePermission(c *gin.Context) error {
	id := c.Param("id")
	permId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req permission.UpdatePermissionInfoRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err = h.rolePermGW.UpdatePermissionInfo(ctx, permId, &req)
	if err != nil {
		return err
	}
	c.JSON(http.StatusOK, gin.H{})
	return nil
}
