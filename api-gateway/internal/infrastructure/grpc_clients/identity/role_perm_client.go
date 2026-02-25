package identity

import (
	idenGW "api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/permission"
	"api-gateway/internal/domain/models/role"
	"api-gateway/pkg/mapper"
	idenPb "api-gateway/pkg/pb/identity"
	"context"

	"github.com/google/uuid"
	"google.golang.org/protobuf/types/known/emptypb"
)

type rolePermGrpcClient struct {
	client idenPb.RolePermissionGrpcClient
}

func ProvideRolePermClient(f *identityClientFactory) idenGW.IRolePermissionService {
	return &rolePermGrpcClient{client: idenPb.NewRolePermissionGrpcClient(f.conn)}
}

// GetAllPermissionsGrouped implements identity.IRolePermissionService.
func (r *rolePermGrpcClient) GetAllPermissionsGrouped(ctx context.Context) ([]*permission.PermissionGroup, error) {
	res, err := r.client.GetAllPermissionsGrouped(ctx, &emptypb.Empty{})
	if err != nil {
		return nil, err
	}

	return mapper.MapPermGroupPbToModel(res.Groups), nil
}

// GetPermissionsByRoleId implements identity.IRolePermissionService.
func (r *rolePermGrpcClient) GetPermissionsByRoleId(ctx context.Context, id uuid.UUID) ([]*permission.PermissionGroup, error) {
	res, err := r.client.GetPermissionsByRoleId(ctx, &idenPb.GetPermissionsByRoleIdRequest{
		Id: id.String(),
	})
	if err != nil {
		return []*permission.PermissionGroup{}, err
	}

	return mapper.MapPermGroupPbToModel(res.Groups), nil
}

// GetPermissionsBySlug implements identity.IRolePermissionService.
func (r *rolePermGrpcClient) GetPermissionsBySlug(ctx context.Context, roleSlug string) ([]string, error) {
	res, err := r.client.GetPermissionsBySlug(ctx, &idenPb.GetPermissionsBySlugRequest{
		RoleSlug: roleSlug,
	})
	if err != nil {
		return []string{}, err
	}
	return res.Permissions, nil
}

// UpdatePermissionInfo implements identity.IRolePermissionService.
func (r *rolePermGrpcClient) UpdatePermissionInfo(ctx context.Context, id uuid.UUID, req *permission.UpdatePermissionInfoRequest) error {
	_, err := r.client.UpdatePermissionInfo(ctx, &idenPb.UpdatePermissionInfoRequest{
		Id:          id.String(),
		Name:        req.Name,
		Description: req.Description,
	})
	return err
}

// UpdateRolePermissions implements identity.IRolePermissionService.
func (r *rolePermGrpcClient) UpdateRolePermissions(ctx context.Context, id uuid.UUID, req *role.UpdateRolePermissionsRequest) error {
	_, err := r.client.UpdateRolePermissions(ctx, &idenPb.UpdateRolePermissionsRequest{
		Id:                  id.String(),
		PermissionsToGrant:  req.PermissionsToGrant,
		PermissionsToRevoke: req.PermissionsToRevoke,
	})
	return err
}
