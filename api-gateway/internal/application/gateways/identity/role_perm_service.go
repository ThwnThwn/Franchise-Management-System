package identity

import (
	"api-gateway/internal/domain/models/permission"
	"context"

	"github.com/google/uuid"
)

type IRolePermissionService interface {
	// 1. Get PermissionsByRoleId
	GetPermissionsByRoleId(ctx context.Context, id uuid.UUID) ([]*permission.RolePermissionGroupRes, error)
	// 1.5. Get Cache (Lazy Loading)
	GetPermissionsBySlug(ctx context.Context, roleSlug string) ([]string, error)
	// 2. Update Info (Name, Description)
	UpdatePermissionInfo(ctx context.Context, id uuid.UUID, req *permission.UpdatePermissionInfoRequest) error
	// 3. Update Permissions for Role
	UpdateRolePermissions(ctx context.Context, id uuid.UUID, req *permission.UpdateRolePermissionsRequest) error
	// 4. Get All Grouped (Cho UI Matrix)
	GetAllPermissionsGrouped(ctx context.Context) ([]*permission.PermissionGroup, error)
}
