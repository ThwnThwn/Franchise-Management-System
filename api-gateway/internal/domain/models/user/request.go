package user

import "api-gateway/internal/domain/models/common"

type CreateUserRequest struct {
	Username    string `json:"username" binding:"required"`
	Email       string `json:"email" binding:"required,email"`
	Phone       string `json:"phone"`
	Address     string `json:"address"`
	FirstName   string `json:"firstName"`
	LastName    string `json:"lastName"`
	Gender      *int32 `json:"gender" binding:"required"`
	DateOfBirth string `json:"dateOfBirth"`
	RoleSlug    string `json:"roleSlug" binding:"required"`
}

type UpdateUserRequest struct {
	Username    string `json:"username"`
	Email       string `json:"email"`
	Phone       string `json:"phone"`
	Address     string `json:"address"`
	FirstName   string `json:"firstName"`
	LastName    string `json:"lastName"`
	Gender      int32  `json:"gender"`
	DateOfBirth string `json:"dateOfBirth"`
	IsBlocked   bool   `json:"isBlocked"`
	RoleSlug    string `json:"roleSlug"`
}

type UpdateUserRoleRequest struct {
	RoleSlug string `json:"roleSlug" binding:"required"`
}

type UpdateMeRequest struct {
	Email       string `json:"email"`
	Phone       string `json:"phone"`
	Address     string `json:"address"`
	FirstName   string `json:"firstName"`
	LastName    string `json:"lastName"`
	Gender      int32  `json:"gender"`
	DateOfBirth string `json:"dateOfBirth"`
}

type ChangePasswordRequest struct {
	OldPassword        string `json:"oldPassword" binding:"required,min=6,max=30"`
	NewPassword        string `json:"newPassword" binding:"required,min=6,max=30"`
	ConfirmNewPassword string `json:"confirmNewPassword" binding:"required,eqfield=NewPassword"`
	ForgetAll          bool   `json:"forgetAll"`
}

type GetAllUsersRequest struct {
	common.PaginationQuery
	RoleSlug string `form:"roleSlug"`
	Keyword  string `form:"keyword"`
}
