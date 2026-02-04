package usecases

import (
	"api-gateway/internal/application/abstractions"
	idenGW "api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/user"
	"context"

	"github.com/google/uuid"
)

type UserUseCase struct {
	servive idenGW.IUserService
}

func NewUserUseCase(service idenGW.IUserService) abstractions.IUserUseCase {
	return &UserUseCase{servive: service}
}

// GetById implements abstractions.IUserUseCase.
func (u *UserUseCase) GetById(ctx context.Context, id uuid.UUID) (*user.UserResponse, error) {
	return u.servive.GetById(ctx, id)
}

func (u *UserUseCase) Update(ctx context.Context, id uuid.UUID, req *user.UpdateUserRequest) (*user.UserResponse, error) {
	return u.servive.Update(ctx, id, req)
}

// ChangePassword implements abstractions.IUserUseCase.
func (u *UserUseCase) ChangePassword(ctx context.Context, id uuid.UUID, req *user.ChangePasswordRequest) (*user.UserResponse, error) {
	// return u.servive.ChangePassword(ctx, id, req)
	return nil, nil
}

// Create implements abstractions.IUserUseCase.
func (u *UserUseCase) Create(ctx context.Context, req *user.CreateUserRequest) (*user.UserResponse, error) {
	return u.servive.Create(ctx, req)
}

// GetAll implements abstractions.IUserUseCase.
func (u *UserUseCase) GetAll(ctx context.Context, req *user.GetAllUsersRequest) (
	common.PaginationResult[*user.UserResponse], error,
) {
	return u.servive.GetAll(ctx, req)
}
