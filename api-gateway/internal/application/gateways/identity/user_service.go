package identity

import (
	"api-gateway/internal/domain/models/user"
	"context"

	"github.com/google/uuid"
)

type IUserService interface {
	GetAll(ctx context.Context) ([]*user.UserResponse, error)
	GetById(ctx context.Context, id uuid.UUID) (*user.UserResponse, error)
	Create(ctx context.Context, req *user.CreateUserRequest) (*user.UserResponse, error)
	Update(ctx context.Context, id uuid.UUID, req *user.UpdateUserRequest) (*user.UserResponse, error)
	ChangePassword(ctx context.Context, id uuid.UUID, req *user.ChangePasswordRequest) (*user.UserResponse, error)
}
