package loyalty

type GetByUserPhoneOrEmailRequest struct {
	PhoneNumber *string `form:"phoneNumber"`
	Email       *string `form:"email"`
}

type GetLoyaltyReportRequest struct {
	FromDate *string `form:"fromDate"`
	ToDate   *string `form:"toDate"`
	TierId   *string `form:"tierId"`
	Format   int32   `form:"format"`
}
