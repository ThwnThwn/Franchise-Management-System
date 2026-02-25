package mapper

import (
	"api-gateway/internal/domain/models/role"
	"api-gateway/internal/domain/models/user"
	"api-gateway/pkg/pb/identity"
	"api-gateway/pkg/utils/stringutils"
	"api-gateway/pkg/utils/timeutils"
)

func MapUserPbToUserModel(userPb *identity.UserResponse) *user.UserResponse {
	return &user.UserResponse{
		Id:          userPb.Id,
		Email:       userPb.Email,
		FirstName:   userPb.FirstName,
		LastName:    userPb.LastName,
		Username:    userPb.Username,
		Phone:       userPb.Phone,
		Address:     userPb.Address,
		Gender:      userPb.Gender,
		DateOfBirth: timeutils.ProtoToTimePtr(userPb.DateOfBirth),

		IsBlocked:   userPb.IsBlocked,
		HomeStoreID: userPb.HomeStoreId,
		AvatarUrl:   stringutils.ToPtr(userPb.AvatarUrl),

		Role: &role.RoleUserResponse{
			Id:   userPb.Role.Id,
			Name: userPb.Role.Name,
		},
		CreatedAt: timeutils.ProtoToTimePtr(userPb.CreatedAt),
		UpdatedAt: timeutils.ProtoToTimePtr(userPb.UpdatedAt),
	}
}
