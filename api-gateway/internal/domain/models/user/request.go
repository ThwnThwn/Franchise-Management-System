package user

import "time"

type UpdateUserRequest struct {
	Id          string     `json:"id"`
	Username    string     `json:"username"`
	Email       string     `json:"email"`
	Phone       string     `json:"phone"`
	Address     string     `json:"address"`
	FirstName   string     `json:"firstName"`
	LastName    string     `json:"lastName"`
	Gender      int32      `json:"gender"`
	DateOfBirth *time.Time `json:"dateOfBirth"`
	IsBlocked   bool       `json:"isBlocked"`
	RoleSlug    string     `json:"roleSlug"`
}
