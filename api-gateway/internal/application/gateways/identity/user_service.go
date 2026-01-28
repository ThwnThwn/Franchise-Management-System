package identity

import (
	"api-gateway/internal/domain/models/user"
	"context"

	"github.com/google/uuid"
)

type IUserService interface {
	GetById(ctx context.Context, id uuid.UUID) (*user.UserResponse, error)
	Update(ctx context.Context, req *user.UpdateUserRequest) (*user.UserResponse, error)
}
