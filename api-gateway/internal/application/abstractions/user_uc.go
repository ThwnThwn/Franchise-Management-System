package abstractions

import (
	"api-gateway/internal/domain/models/user"
	"context"

	"github.com/google/uuid"
)

type IUserUseCase interface {
	GetById(ctx context.Context, id uuid.UUID) (*user.UserResponse, error)
}
