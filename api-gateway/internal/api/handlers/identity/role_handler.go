package identity

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

// GetAll
// @Summary      Get All Roles
// @Description  Retrieve a list of all defined roles in the system.
// @Tags         Roles
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Success      200  {object}  map[string]interface{} "List of roles"
// @Failure      500  {object}  map[string]string      "Internal server error"
// @Router       /roles [get]
func (h *RoleHandler) GetAll(c *gin.Context) error {
	ctx := c.Request.Context()
	res, err := h.roleGW.GetAll(ctx)
	if err != nil {
		return err
	}
	c.JSON(http.StatusOK, res)
	return nil
}

// Create
// @Summary      Create New Role
// @Description  Define a new role with a specific name and description.
// @Tags         Roles
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        request body     role.CreateRoleRequest  true  "Role details"
// @Success      200     {object} map[string]interface{}        "Success"
// @Failure      400     {object} map[string]string             "Validation error"
// @Router       /roles [post]
func (h *RoleHandler) Create(c *gin.Context) error {
	var req role.CreateRoleRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	if err := h.roleGW.Create(ctx, &req); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// Update
// @Summary      Update Role
// @Description  Update details (name, description) of an existing role.
// @Tags         Roles
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                  true  "Role UUID"
// @Param        request  body     role.UpdateRoleRequest  true  "Updated details"
// @Success      200      {object} map[string]interface{}        "Success"
// @Failure      400      {object} map[string]string             "Validation error or Invalid ID"
// @Router       /roles/{id} [patch]
func (h *RoleHandler) Update(c *gin.Context) error {
	id := c.Param("id")
	roleId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req role.UpdateRoleRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	if err := h.roleGW.Update(ctx, roleId, &req); err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}

// --- Role Perm ---

// GetPermissionsByRoleId
// @Summary      Get Role Permissions
// @Description  Retrieve all permissions assigned to a specific role.
// @Tags         Roles
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id   path      string  true  "Role UUID"
// @Success      200  {object}  map[string]interface{} "List of permissions"
// @Failure      400  {object}  map[string]string      "Invalid ID format"
// @Router       /roles/{id}/permissions [get]
func (h *RoleHandler) GetPermissionsByRoleId(c *gin.Context) error {
	id := c.Param("id")
	roleId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	ctx := c.Request.Context()
	res, err := h.rolePermGW.GetPermissionsByRoleId(ctx, roleId)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, res)
	return nil
}

// UpdateRolePermissions
// @Summary      Assign Permissions to Role
// @Description  Update the list of permissions assigned to a specific role.
// @Tags         Roles
// @Accept       json
// @Produce      json
// @Security     BearerAuth
// @Param        id       path     string                         true  "Role UUID"
// @Param        request  body     role.UpdateRolePermissionsRequest true "List of Permission IDs"
// @Success      200      {object} map[string]interface{}         "Success"
// @Failure      400      {object} map[string]string              "Validation error or Invalid ID"
// @Router       /roles/{id}/permissions [put]
func (h *RoleHandler) UpdateRolePermissions(c *gin.Context) error {
	id := c.Param("id")
	roleId, err := uuid.Parse(id)
	if err != nil {
		return errorcode.ErrInvalidId
	}

	var req role.UpdateRolePermissionsRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		return validation.TranslateValidationError(err)
	}

	ctx := c.Request.Context()
	err = h.rolePermGW.UpdateRolePermissions(ctx, roleId, &req)
	if err != nil {
		return err
	}

	c.JSON(http.StatusOK, gin.H{})
	return nil
}
