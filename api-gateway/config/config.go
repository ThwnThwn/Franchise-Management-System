package config

import (
	"log"
	"os"

	"github.com/joho/godotenv"
)

type Config struct {
	Port         string
	SalesService string
}

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
		Port:         getEnv("GATEWAY_PORT", "8080"),
		SalesService: getEnv("SALES_SERVICE_URL", "http://localhost:5001"),
	}
}

func getEnv(key, fallback string) string {
	if value, exists := os.LookupEnv(key); exists {
		return value
	}
	return fallback
}
