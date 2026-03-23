namespace SalesService.Application.Interfaces;

public interface IAesEncryptionService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherTextBase64);

    string GenerateMaskedAccountNumber(string accountNumber);
}