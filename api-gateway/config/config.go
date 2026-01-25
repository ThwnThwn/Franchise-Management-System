package config

import (
	"log"
	"os"
	"strconv"

	"github.com/joho/godotenv"
)

type Config struct {
	Port               string
	CalcServiceUrl     CalcServiceUrl
	IdentityServiceUrl IdentityServiceUrl
	ProductServiceUrl  ProductServiceUrl
	SalesServiceUrl    SalesServiceUrl

	JwtRefreshTokenExpDays int
	JwtSecret              string
}

type CalcServiceUrl string
type IdentityServiceUrl string
type ProductServiceUrl string
type SalesServiceUrl string

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
		Port:               getEnv("GATEWAY_PORT", "8080"),
		CalcServiceUrl:     CalcServiceUrl(getEnv("CALC_SERVICE_URL", "localhost:5099")),
		IdentityServiceUrl: IdentityServiceUrl(getEnv("IDENTITY_SERVICE_URL", "localhost:5001")),
		ProductServiceUrl:  ProductServiceUrl(getEnv("PRODUCT_SERVICE_URL", "localhost:5002")),
		SalesServiceUrl:    SalesServiceUrl(getEnv("SALES_SERVICE_URL", "localhost:5003")),

		JwtRefreshTokenExpDays: getEnvInt("JWT_REFRESH_TOKEN_EXPIRATION_DAYS", 7),
		JwtSecret:              getEnv("JWT_SECRET", ""),
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
