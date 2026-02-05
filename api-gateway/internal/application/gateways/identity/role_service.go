package identity

import (
	"api-gateway/internal/domain/models/role"
	"context"

	"github.com/google/uuid"
)

type IRoleService interface {
	Create(ctx context.Context, req *role.CreateRoleRequest) error
	GetById(ctx context.Context, id uuid.UUID) (*role.RoleResponse, error)
	GetAll(ctx context.Context) ([]*role.RoleResponse, error)
	Update(ctx context.Context, id uuid.UUID, req *role.UpdateRoleRequest) error
	Delete(ctx context.Context, id uuid.UUID) error
}
