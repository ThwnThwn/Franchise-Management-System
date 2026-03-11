package shift

type AssignStaffRequest struct {
	UserIds        []string `json:"userIds" binding:"required"`
	ForceOverride  bool     `json:"forceOverride"`
	OverrideReason string   `json:"overrideReason"`
}
