package permission

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
