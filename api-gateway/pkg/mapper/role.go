package mapper

import (
	"api-gateway/internal/domain/models/role"
	"api-gateway/pkg/pb/identity"
	"api-gateway/pkg/utils/timeutils"
)

func MapRolePbToRoleModel(rolePb *identity.RoleResponse) *role.RoleResponse {
	return &role.RoleResponse{
		Id:          rolePb.Id,
		Name:        rolePb.Name,
		Slug:        rolePb.Slug,
		Description: rolePb.Description,
		CreatedAt:   timeutils.ProtoToTimePtr(rolePb.CreatedAt),
		UpdatedAt:   timeutils.ProtoToTimePtr(rolePb.UpdatedAt),
	}
}
