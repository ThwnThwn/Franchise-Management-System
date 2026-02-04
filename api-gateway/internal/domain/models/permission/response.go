package permission

type RolePermissionGroupRes struct {
	Resource    string               `json:"resource"`
	Permissions []*RolePermissionRes `json:"permissions"`
}

type RolePermissionRes struct {
	Id          string `json:"id"`
	Slug        string `json:"slug"`
	Name        string `json:"name"`
	Description string `json:"description"`
	IsActived   bool   `json:"isActived"`
}

type PermissionGroup struct {
	Resource    string              `json:"resource"`
	Permissions []*PermissionDetail `json:"permissions"`
}

type PermissionDetail struct {
	Id          string `json:"id"`
	Slug        string `json:"slug"`
	Name        string `json:"name"`
	Description string `json:"description"`
}
