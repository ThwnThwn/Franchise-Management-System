package identity

import (
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/user"
	"context"

	"github.com/google/uuid"
)

type IUserService interface {
	GetAll(ctx context.Context, req *user.GetAllUsersRequest) (
		common.PaginationResult[*user.UserResponse], error)
	GetById(ctx context.Context, id uuid.UUID) (*user.UserResponse, error)
	GetByIds(ctx context.Context, ids []string) ([]*user.UserResponse, error)
	Create(ctx context.Context, req *user.CreateUserRequest) (*user.UserResponse, error)
	Update(ctx context.Context, id uuid.UUID, req *user.UpdateUserRequest) (*user.UserResponse, error)
	ChangePassword(ctx context.Context, accessToken string, req *user.ChangePasswordRequest) (*user.UserResponse, error)

	UploadMyAvatar(ctx context.Context, imageData []byte) error
	DeleteMyAvatar(ctx context.Context) error
}
