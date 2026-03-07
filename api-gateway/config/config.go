package config

type Config struct {
	Logger Logger

	Redis Redis
	VNPay VNPay

	Port           string
	FrontEndOrigin string

	CalcServiceUrl       CalcServiceUrl
	IdentityServiceUrl   IdentityServiceUrl
	ProductServiceUrl    ProductServiceUrl
	SalesServiceUrl      SalesServiceUrl
	FranchiseServiceUrl  FranchiseServiceUrl
	InventoryServiceUrl  InventoryServiceUrl
	EngagementServiceUrl EngagementServiceUrl
	SupplierServiceUrl   SupplierServiceUrl

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
	ReturnUrl       string
	ClientReturnUrl string
	IpnUrl          string

	WalletReturnUrl       string
	WalletClientReturnUrl string
	WalletIpnUrl          string

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
type EngagementServiceUrl string
type SupplierServiceUrl string
