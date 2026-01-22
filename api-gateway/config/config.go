package config

import (
	"os"

	"github.com/joho/godotenv"
)

type Config struct {
	Port         string
	SalesService string
}

func LoadConfig() *Config {
	_ = godotenv.Load()

	return &Config{
		Port:         getEnv("PORT", "8080"),
		SalesService: getEnv("SALES_SERVICE_URL", "http://localhost:5001"),
	}
}

func getEnv(key, fallback string) string {
	if value, exists := os.LookupEnv(key); exists {
		return value
	}
	return fallback
}
