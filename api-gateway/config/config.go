package config

import (
	"log"
	"os"

	"github.com/joho/godotenv"
)

type Config struct {
	Port           string
	CalcServiceUrl string
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
		Port:           getEnv("GATEWAY_PORT", "8080"),
		CalcServiceUrl: getEnv("CALC_SERVICE_URL", "localhost:5099"),
	}
}

func getEnv(key, fallback string) string {
	if value, exists := os.LookupEnv(key); exists {
		return value
	}
	return fallback
}
