package mapper

import (
	"api-gateway/internal/domain/models/loyalty"
	"api-gateway/internal/domain/models/role"
	"api-gateway/internal/domain/models/user"
	"api-gateway/pkg/pb/identity"
	"api-gateway/pkg/utils/stringutils"
	"api-gateway/pkg/utils/timeutils"
)

func MapUserPbToUserModel(userPb *identity.UserResponse) *user.UserResponse {
	if userPb == nil {
		return nil
	}

	var loyaltyRes *loyalty.LoyaltyResponse = nil
	if userPb.Loyalty != nil {
		var currentTierRes *loyalty.MembershipTier = nil
		if userPb.Loyalty.CurrentTier != nil {
			t := userPb.Loyalty.CurrentTier
			currentTierRes = &loyalty.MembershipTier{
				Id:               t.Id,
				Name:             t.Name,
				MinSpendRequired: t.MinSpendRequired,
				DiscountRate:     t.DiscountRate,
				AmountPerPoint:   t.AmountPerPoint,
				ValuePerPoint:    t.ValuePerPoint,
			}
		}

		loyaltyRes = &loyalty.LoyaltyResponse{
			Id:              userPb.Loyalty.Id,
			AvailablePoints: userPb.Loyalty.AvailablePoints,
			TotalSpent:      userPb.Loyalty.TotalSpent,
			CurrentTier:     currentTierRes,
		}
	}

	var roleRes *role.RoleUserResponse = nil
	if userPb.Role != nil {
		roleRes = &role.RoleUserResponse{
			Id:   userPb.Role.Id,
			Name: userPb.Role.Name,
			Slug: userPb.Role.Slug,
		}
	}

	return &user.UserResponse{
		Id:          userPb.Id,
		Username:    userPb.Username,
		Email:       userPb.Email,
		Phone:       userPb.Phone,
		Address:     userPb.Address,
		FirstName:   userPb.FirstName,
		LastName:    userPb.LastName,
		Gender:      userPb.Gender,
		DateOfBirth: timeutils.ProtoToTimePtr(userPb.DateOfBirth),

		AvatarUrl: stringutils.ToPtr(userPb.AvatarUrl),
		IsBlocked: userPb.IsBlocked,
		Role:      roleRes,

		HomeStoreId:       userPb.HomeStoreId,
		CurrentHourlyWage: userPb.CurrentHourlyWage,

		Loyalty:    loyaltyRes,
		OrderCount: userPb.OrderCount,

		CreatedAt: timeutils.ProtoToTimePtr(userPb.CreatedAt),
		UpdatedAt: timeutils.ProtoToTimePtr(userPb.UpdatedAt),
	}
}
