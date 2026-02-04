package role

import "time"

type RoleUserResponse struct {
	Id   string `json:"id"`
	Name string `json:"name"`
}

type RoleResponse struct {
	Id          string     `json:"id"`
	Name        string     `json:"name"`
	Slug        string     `json:"slug"`
	Description string     `json:"description"`
	CreatedAt   *time.Time `json:"createdAt"`
	UpdatedAt   *time.Time `json:"updatedAt"`
}

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
