package roleslug

type RoleSlug string

const (
	Admin    RoleSlug = "admin"
	Manager  RoleSlug = "manager"
	Staff    RoleSlug = "staff"
	Customer RoleSlug = "customer"
)
