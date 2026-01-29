package identity

import (
	idenGW "api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/user"
	"api-gateway/pkg/mapper"
	idenPb "api-gateway/pkg/pb/identity"
	"context"

	"github.com/google/uuid"
	"google.golang.org/protobuf/types/known/emptypb"
)

type UserGrpcClient struct {
	client idenPb.UserGrpcClient
}

func ProvideUserClient(f *identityClientFactory) idenGW.IUserService {
	return &UserGrpcClient{client: idenPb.NewUserGrpcClient(f.conn)}
}

// GetAll implements identity.IUserService.
func (u *UserGrpcClient) GetAll(ctx context.Context) ([]*user.UserResponse, error) {
	res, err := u.client.GetAll(ctx, &emptypb.Empty{})
	if err != nil {
		return nil, err
	}

	users := make([]*user.UserResponse, 0, len(res.Users))
	for _, user := range res.Users {
		users = append(users, mapper.MapUserPbToUserModel(user))
	}

	return users, nil
}

func (u *UserGrpcClient) GetById(ctx context.Context, id uuid.UUID) (*user.UserResponse, error) {
	res, err := u.client.GetById(ctx, &idenPb.GetUserByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}
	return mapper.MapUserPbToUserModel(res), nil
}

// Create implements identity.IUserService.
func (u *UserGrpcClient) Create(ctx context.Context, req *user.CreateUserRequest) (*user.UserResponse, error) {
	res, err := u.client.AdminCreate(ctx, &idenPb.AdminCreateUserRequest{
		Email:       req.Email,
		Username:    req.Username,
		Phone:       req.Phone,
		Address:     req.Address,
		RoleSlug:    req.RoleSlug,
		FirstName:   req.FirstName,
		LastName:    req.LastName,
		Gender:      req.Gender,
		DateOfBirth: req.DateOfBirth,
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapUserPbToUserModel(res), nil
}

// Update implements identity.IUserService.
func (u *UserGrpcClient) Update(ctx context.Context, id uuid.UUID, req *user.UpdateUserRequest) (*user.UserResponse, error) {
	res, err := u.client.Update(ctx, &idenPb.UpdateUserRequest{
		Id:          id.String(),
		Username:    req.Username,
		Phone:       req.Phone,
		Address:     req.Address,
		Gender:      &req.Gender,
		DateOfBirth: req.DateOfBirth,
		IsBlocked:   &req.IsBlocked,
		Email:       req.Email,
		FirstName:   req.FirstName,
		LastName:    req.LastName,
		RoleSlug:    req.RoleSlug,
	})
	if err != nil {
		return nil, err
	}
	return mapper.MapUserPbToUserModel(res), nil
}

// ChangePassword implements identity.IUserService.
func (u *UserGrpcClient) ChangePassword(ctx context.Context, id uuid.UUID, req *user.ChangePasswordRequest) (*user.UserResponse, error) {
	res, err := u.client.ChangePassword(ctx, &idenPb.ChangePasswordRequest{
		Id:                 id.String(),
		OldPassword:        req.OldPassword,
		NewPassword:        req.NewPassword,
		ConfirmNewPassword: req.ConfirmNewPassword,
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapUserPbToUserModel(res), nil
}
