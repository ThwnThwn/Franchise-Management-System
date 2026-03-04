package point

type LogResponse struct {
	Id            string `json:"id"`
	UserId        string `json:"userId"`
	Amount        string `json:"amount"`
	BalanceBefore string `json:"balanceBefore"`
	BalanceAfter  string `json:"balanceAfter"`
	ActionType    string `json:"actionType"`
	ReasonCode    string `json:"reasonCode"`
	ReferenceId   string `json:"referenceId"`
	Description   string `json:"description"`
	ExpiredAt     string `json:"expiredAt"`
	CreatedAt     string `json:"createdAt"`
}
