package abstractions

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/user"
	"context"

	"github.com/google/uuid"
)

type IUserUseCase interface {
	GetAll(ctx context.Context, req *user.GetAllUsersRequest) (
		common.PaginationResult[*user.UserResponse], error,
	)
	GetById(ctx context.Context, id uuid.UUID) (*user.UserResponse, error)
	Create(ctx context.Context, req *user.CreateUserRequest) (*user.UserResponse, error)
	Update(ctx context.Context, id uuid.UUID, req *user.UpdateUserRequest) (*user.UserResponse, error)
	ChangePassword(ctx context.Context, id uuid.UUID, req *user.ChangePasswordRequest) (*user.UserResponse, error)
}
