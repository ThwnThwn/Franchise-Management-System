package identity

import (
	idenGW "api-gateway/internal/application/gateways/identity"
	"api-gateway/internal/domain/models/role"
	"api-gateway/internal/domain/models/user"
	idenPb "api-gateway/pkg/pb/identity"
	"api-gateway/pkg/utils/timeutils"
	"context"

	"github.com/google/uuid"
)

type UserGrpcClient struct {
	client idenPb.UserGrpcClient
}

func ProvideUserClient(f *identityClientFactory) idenGW.IUserService {
	return &UserGrpcClient{client: idenPb.NewUserGrpcClient(f.conn)}
}

func (u *UserGrpcClient) GetUserById(ctx context.Context, id uuid.UUID) (*user.UserResponse, error) {
	res, err := u.client.GetById(ctx, &idenPb.GetUserByIdRequest{Id: id.String()})
	if err != nil {
		return nil, err
	}
	return &user.UserResponse{
		Id:          res.Id,
		Email:       res.Email,
		FirstName:   res.FirstName,
		LastName:    res.LastName,
		Username:    res.Username,
		Phone:       res.Phone,
		Address:     res.Address,
		Gender:      res.Gender,
		DateOfBirth: timeutils.ProtoToTimePtr(res.DateOfBirth),
		IsBlocked:   res.IsBlocked,
		Role: &role.RoleUserResponse{
			Id:   res.Role.Id,
			Name: res.Role.Name,
		},
		CreatedAt: timeutils.ProtoToTimePtr(res.CreatedAt),
		UpdatedAt: timeutils.ProtoToTimePtr(res.UpdatedAt),
	}, nil
}
