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
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD

func LoadConfig() *Config {
	err := godotenv.Load(".env")
	if err != nil {
		errRoot := godotenv.Load("../.env")
		if errRoot != nil {
			log.Println("Warning: No .env file found. Using OS environment variables.")
		} else {
			log.Println("Loaded .env from Root folder")
		}
	}

	cfg := &Config{
		Logger: Logger{
			LogLevel:    getEnv("GATEWAY_LOGGER_LOG_LEVEL", "debug"),
			FileLogName: getEnv("GATEWAY_LOGGER_FILE_LOG_NAME", "./logs/dev.log"),
			MaxBackups:  getEnvInt("GATEWAY_LOGGER_MAX_BACKUPS", 3),
			MaxAge:      getEnvInt("GATEWAY_LOGGER_MAX_AGE", 30),
			MaxSize:     getEnvInt("GATEWAY_LOGGER_MAX_SIZE", 100),
			Compress:    getEnvBool("GATEWAY_LOGGER_COMPRESS", true),
		},

		Redis: Redis{
			Host:       getEnv("REDIS_HOST", "127.0.0.1"),
			Port:       getEnvInt("REDIS_PORT", 6379),
			Password:   getEnv("REDIS_PASSWORD", ""),
			BlaclistDb: getEnvInt("REDIS_BLACKLIST_DB", 0),
			PermDb:     getEnvInt("REDIS_PERM_DB", 1),
		},

		Port:           getEnv("GATEWAY_PORT", "8080"),
		FrontEndOrigin: getEnv("FRONTEND_ORIGIN", "http://localhost:3000"),

		CalcServiceUrl: CalcServiceUrl(getEnv("CALC_SERVICE_URL", "localhost:5099")),

		IdentityServiceUrl:  IdentityServiceUrl(getEnvUrl("IDENTITY_SERVICE_URL", "localhost:5001")),
		ProductServiceUrl:   ProductServiceUrl(getEnvUrl("PRODUCT_SERVICE_URL", "localhost:5002")),
		SalesServiceUrl:     SalesServiceUrl(getEnvUrl("SALES_SERVICE_URL", "localhost:5003")),
		InventoryServiceUrl: InventoryServiceUrl(getEnvUrl("INVENTORY_SERVICE_URL", "localhost:5005")),
		FranchiseServiceUrl: FranchiseServiceUrl(getEnvUrl("FRANCHISE_SERVICE_URL", "localhost:5006")),

		JwtRefreshTokenExpDays:          getEnvInt("JWT_REFRESH_TOKEN_EXPIRATION_DAYS", 7),
		JwtSecret:                       getEnv("JWT_SECRET", ""),
		JwtAccessTokenExpirationMinutes: getEnvInt("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES", 15),

		MaxUploadSizeMb:       getEnvInt("MAX_UPLOAD_SIZE_MB", 10),
		GatewayTimeoutSeconds: getEnvInt("GATEWAY_TIMEOUT_SECONDS", 180),
	}
	return cfg
}

func getEnv(key, fallback string) string {
	if value, exists := os.LookupEnv(key); exists {
		return value
	}
	return fallback
}

func getEnvInt(key string, fallback int) int {
	if value, exists := os.LookupEnv(key); exists {
		if val, err := strconv.Atoi(value); err == nil {
			return val
		}
	}
	return fallback
}

// func getEnvFloat32(key string, fallback float32) float32 {
// 	if value, exists := os.LookupEnv(key); exists {
// 		if val, err := strconv.ParseFloat(value, 32); err == nil {
// 			return float32(val)
// 		}
// 	}
// 	return fallback
// }

func getEnvUrl(key, fallback string) string {
	if value, exists := os.LookupEnv(key); exists {
		address := strings.TrimPrefix(value, "http://")
		address = strings.TrimPrefix(address, "https://")
		return address
	}
	return fallback
}

func getEnvBool(key string, fallback bool) bool {
	if value, exists := os.LookupEnv(key); exists {
		if val, err := strconv.ParseBool(value); err == nil {
			return val
		}
	}
	return fallback
}
=======
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
=======
type EngagementServiceUrl string
>>>>>>> 62648aa7a56b46631a1a4ae7906bbbe8dfe772f9
=======
type EngagementServiceUrl string
>>>>>>> 67a394a75859432ebb4e70c583813a2d5a7dff05
