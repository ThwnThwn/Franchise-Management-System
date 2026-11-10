package supplier

type SupplierSummary struct {
	Id            string `json:"id"`
	Code          string `json:"code"`
	Name          string `json:"name"`
	ContactPerson string `json:"contactPerson"`
	Phone         string `json:"phone"`
	IsActive      bool   `json:"isActive"`
}

type SupplierDetail struct {
	SupplierSummary

	Email     string `json:"email"`
	Address   string `json:"address"`
	TaxCode   string `json:"taxCode"`
	CreatedAt string `json:"createdAt"`
	UpdatedAt string `json:"updatedAt"`
}

type SupplierBaseRes struct {
	Id      string `json:"id,omitempty"`
	Code    string `json:"code,omitempty"`
	Message string `json:"message"`
}
