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
