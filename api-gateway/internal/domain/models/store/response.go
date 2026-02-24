package store

type StoreDetailResponse struct {
	Id        string   `json:"id"`
	Name      string   `json:"name"`
	Address   string   `json:"address"`
	Phone     *string  `json:"phone"`
	ImageUrl  *string  `json:"imageUrl"`
	Latitude  *float64 `json:"latitude"`
	Longitude *float64 `json:"longitude"`
	IsOpenNow bool     `json:"isOpenNow"` // Tính toán dựa trên StoreHours ngay lúc gọi
}
