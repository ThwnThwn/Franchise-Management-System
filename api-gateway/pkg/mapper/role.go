package mapper

import (
	"api-gateway/internal/domain/models/permission"
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

func MapPermGroupPbToModel(groupPb []*identity.PermissionGroup) []*permission.PermissionGroup {
	result := make([]*permission.PermissionGroup, 0, len(groupPb))
	for _, g := range groupPb {
		groupPerms := make([]*permission.PermissionDetail, 0, len(g.Permissions))

		for _, p := range g.Permissions {
			groupPerms = append(groupPerms, &permission.PermissionDetail{
				Id:          p.Id,
				Slug:        p.Slug,
				Name:        p.Name,
				Description: p.Description,
			})
		}

		result = append(result, &permission.PermissionGroup{
			Resource:    g.Resource,
			Permissions: groupPerms,
		})
	}
	return result
}
