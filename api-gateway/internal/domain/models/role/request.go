package role

type CreateRoleRequest struct {
	Name        string `json:"name"`
	Slug        string `json:"slug"`
	Description string `json:"description"`
}

type UpdateRoleRequest struct {
	Name        *string `json:"name"`
	Description *string `json:"description"`
}

type UpdateRolePermissionsRequest struct {
	PermissionsToGrant  []string `json:"permissionsToGrant"`
	PermissionsToRevoke []string `json:"permissionsToRevoke"`
}
