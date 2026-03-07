package shift

type ApplyForShiftRequest struct {
	Note string `json:"note"`
}

type ReviewApplicationRequest struct {
	IsApproved bool   `json:"isApproved"`
	ReviewNote string `json:"reviewNote"`
}
