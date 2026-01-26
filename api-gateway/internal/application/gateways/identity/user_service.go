package identity

import (
	"api-gateway/internal/domain/models/user"
	"context"

	"github.com/google/uuid"
)

type IUserService interface {
	GetUserById(ctx context.Context, id uuid.UUID) (*user.UserResponse, error)
}
