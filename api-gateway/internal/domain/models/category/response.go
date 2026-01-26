package category

type Category struct {
	Id          string `json:"id"`
	Name        string `json:"name"`
	Description string `json:"description"`
	ImageUrl    string `json:"imageUrl"`
	IsActive    bool   `json:"isActive"`
}
