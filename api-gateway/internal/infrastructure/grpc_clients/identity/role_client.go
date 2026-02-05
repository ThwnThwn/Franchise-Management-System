package identity

import (
	idenGW "api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/role"
	"api-gateway/pkg/mapper"
	idenPb "api-gateway/pkg/pb/identity"
	"api-gateway/pkg/utils/stringutils"
	"context"

	"github.com/google/uuid"
	"google.golang.org/protobuf/types/known/emptypb"
)

type roleGrpcClient struct {
	client idenPb.RoleGrpcClient
}

func ProvideRoleClient(f *identityClientFactory) idenGW.IRoleService {
	return &roleGrpcClient{client: idenPb.NewRoleGrpcClient(f.conn)}
}

// Create implements identity.IRoleService.
func (r *roleGrpcClient) Create(ctx context.Context, req *role.CreateRoleRequest) error {
	_, err := r.client.Create(ctx, &idenPb.CreateRoleRequest{
		Name:        req.Name,
		Slug:        req.Slug,
		Description: req.Description,
	})
	return err
}

// Delete implements identity.IRoleService.
func (r *roleGrpcClient) Delete(ctx context.Context, id uuid.UUID) error {
	_, err := r.client.Delete(ctx, &idenPb.DeleteRoleRequest{Id: id.String()})
	return err
}

// GetAll implements identity.IRoleService.
func (r *roleGrpcClient) GetAll(ctx context.Context) ([]*role.RoleResponse, error) {
	res, err := r.client.GetAll(ctx, &emptypb.Empty{})
	if err != nil {
		return []*role.RoleResponse{}, err
	}
	result := make([]*role.RoleResponse, 0, len(res.Roles))
	for _, r := range res.Roles {
		result = append(result, mapper.MapRolePbToRoleModel(r))
	}

	return result, nil
}

// GetById implements identity.IRoleService.
func (r *roleGrpcClient) GetById(ctx context.Context, id uuid.UUID) (*role.RoleResponse, error) {
	res, err := r.client.GetById(ctx, &idenPb.GetRoleByIdRequest{
		Id: id.String(),
	})
	if err != nil {
		return nil, err
	}
	return mapper.MapRolePbToRoleModel(res), nil
}

// Update implements identity.IRoleService.
func (r *roleGrpcClient) Update(ctx context.Context, id uuid.UUID, req *role.UpdateRoleRequest) error {
	_, err := r.client.Update(ctx, &idenPb.UpdateRoleRequest{
		Id:          id.String(),
		Name:        stringutils.GetString(req.Name),
		Description: stringutils.GetString(req.Description),
	})
	return err
}
