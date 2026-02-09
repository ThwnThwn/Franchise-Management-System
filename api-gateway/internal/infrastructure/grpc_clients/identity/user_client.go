package identity

import (
	idenGW "api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/common"
	"api-gateway/internal/domain/models/user"
	"api-gateway/pkg/mapper"
	idenPb "api-gateway/pkg/pb/identity"
	"context"

	"github.com/google/uuid"
)

type UserGrpcClient struct {
	client idenPb.UserGrpcClient
}

func ProvideUserClient(f *identityClientFactory) idenGW.IUserService {
	return &UserGrpcClient{client: idenPb.NewUserGrpcClient(f.conn)}
}

// GetAll implements identity.IUserService.
func (u *UserGrpcClient) GetAll(ctx context.Context, req *user.GetAllUsersRequest) (
	common.PaginationResult[*user.UserResponse], error,
) {
	res, err := u.client.GetAll(ctx, &idenPb.GetAllUserOrWithRoleRequest{
		RoleSlug: req.RoleSlug,
		Page:     req.Page,
		PageSize: req.PageSize,
		Keyword:  req.Keyword,
	})
	if err != nil {
		return common.PaginationResult[*user.UserResponse]{}, err
	}

	users := make([]*user.UserResponse, 0, len(res.Users))
	for _, user := range res.Users {
		users = append(users, mapper.MapUserPbToUserModel(user))
	}

	return common.PaginationResult[*user.UserResponse]{
		Items:      users,
		TotalCount: res.TotalCount,
		TotalPages: res.TotalPages,
	}, nil
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
func (u *UserGrpcClient) ChangePassword(ctx context.Context, accessToken string, req *user.ChangePasswordRequest) (*user.UserResponse, error) {
	res, err := u.client.ChangePassword(ctx, &idenPb.ChangePasswordRequest{
		OldPassword:        req.OldPassword,
		NewPassword:        req.NewPassword,
		ConfirmNewPassword: req.ConfirmNewPassword,
		AccessToken:        accessToken,
		ForgetAll:          &req.ForgetAll,
	})
	if err != nil {
		return nil, err
	}

	return mapper.MapUserPbToUserModel(res), nil
}
