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

// type GetUserCurrentShiftResponse struct {
// 	HasShiftToday bool    `json:"hasShiftToday"`
// 	StoreId       string  `json:"storeId"` // ID cửa hàng phải làm hôm nay
// 	StoreName     string  `json:"storeName"`
// 	ShiftStart    string  `json:"shiftStart"` // "07:00"
// 	ShiftEnd      string  `json:"shiftEnd"`   // "12:00"
// 	StoreLat      float64 `json:"storeLat"`   // Để App so sánh GPS khi check-in
// 	StoreLong     float64 `json:"storeLong"`
// }
