package identity

import (
	idenGW "api-gateway/internal/application/gateways/identity"
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

func (u *UserGrpcClient) GetById(ctx context.Context, id uuid.UUID) (*user.UserResponse, error) {
	res, err := u.client.GetById(ctx, &idenPb.GetUserByIdRequest{Id: id.String()})
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
		Gender:      req.Gender,
		DateOfBirth: req.DateOfBirth.String(),
		IsBlocked:   req.IsBlocked,
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
