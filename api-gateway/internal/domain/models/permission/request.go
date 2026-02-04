package permission

type UpdatePermissionInfoRequest struct {
	Name        *string `json:"name"`
	Description *string `json:"description"`
}

type UpdateRolePermissionsRequest struct {
	PermissionsToGrant  []string `json:"permissionsToGrant"`
	PermissionsToRevoke []string `json:"permissionsToRevoke"`
}
