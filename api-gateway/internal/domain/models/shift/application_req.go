package shift

type ApplyForShiftRequest struct {
	Note string `json:"note"`
}

type ReviewApplicationRequest struct {
	IsApproved bool   `json:"isApproved"`
	ReviewedBy string `json:"reviewedBy"` // Manager ID
	ReviewNote string `json:"reviewNote"`
}
