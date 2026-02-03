package config

import (
	"log"
	"os"
	"strconv"
	"strings"

	"github.com/joho/godotenv"
)

type Config struct {
	Logger Logger

	Redis Redis

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
	Host     string
	Port     int
	Password string
}

type CalcServiceUrl string
type IdentityServiceUrl string
type ProductServiceUrl string
type SalesServiceUrl string
type FranchiseServiceUrl string
type InventoryServiceUrl string

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

	return &Config{
		Logger: Logger{
			LogLevel:    getEnv("LOGGER_LOG_LEVEL", "debug"),
			FileLogName: getEnv("LOGGER_FILE_LOG_NAME", "./logs/dev.log"),
			MaxBackups:  getEnvInt("LOGGER_MAX_BACKUPS", 3),
			MaxAge:      getEnvInt("LOGGER_MAX_AGE", 30),
			MaxSize:     getEnvInt("LOGGER_MAX_SIZE", 100),
			Compress:    getEnvBool("LOGGER_COMPRESS", true),
		},

		Redis: Redis{
			Host:     getEnv("REDIS_HOST", "127.0.0.1"),
			Port:     getEnvInt("REDIS_PORT", 6379),
			Password: getEnv("REDIS_PASSWORD", ""),
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
	}
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
