package config

type Config struct {
	Logger Logger

	Redis Redis
	VNPay VNPay

	Port           string
	FrontEndOrigin string

	CalcServiceUrl      CalcServiceUrl
	IdentityServiceUrl  IdentityServiceUrl
	ProductServiceUrl   ProductServiceUrl
	SalesServiceUrl     SalesServiceUrl
	FranchiseServiceUrl FranchiseServiceUrl
	InventoryServiceUrl InventoryServiceUrl

	JwtRefreshTokenExpDays          int
	JwtSecret                       string
	JwtAccessTokenExpirationMinutes int

	MaxUploadSizeMb       int
	GatewayTimeoutSeconds int
}

type Logger struct {
	LogLevel    string
	FileLogName string
	MaxBackups  int
	MaxAge      int
	MaxSize     int
	Compress    bool
}

type Redis struct {
	Host       string
	Port       int
	Password   string
	BlaclistDb int
	PermDb     int
}

type VNPay struct {
	// 1. Luồng Khách hàng (SalesService)
	ReturnUrl       string
	ClientReturnUrl string
	IpnUrl          string

	// 2. Luồng Nhập hàng (SupplierService)
	SupplierReturnUrl       string
	SupplierClientReturnUrl string
	SupplierIpnUrl          string
}

type CalcServiceUrl string
type IdentityServiceUrl string
type ProductServiceUrl string
type SalesServiceUrl string
type FranchiseServiceUrl string
type InventoryServiceUrl string
