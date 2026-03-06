using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SalesService.Application.Interfaces;

namespace SalesService.Infrastructure.Security;

public class AesEncryptionService(IConfiguration configuration) : IAesEncryptionService
{
    private readonly string _secretKey = configuration["AES_SECRET_KEY"]
            ?? throw new InvalidOperationException("Missing AES_SECRET_KEY in environment variables!");

    private const int NonceSize = 12;
    private const int TagSize = 16;

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        byte[] key = Convert.FromBase64String(_secretKey);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        byte[] cipherBytes = new byte[plainBytes.Length];
        byte[] tag = new byte[TagSize];

        using (var aesGcm = new AesGcm(key, TagSize))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        byte[] encryptedData = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, encryptedData, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, encryptedData, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, encryptedData, NonceSize + TagSize, cipherBytes.Length);

        return Convert.ToBase64String(encryptedData);
    }

    public string Decrypt(string cipherTextBase64)
    {
        if (string.IsNullOrEmpty(cipherTextBase64)) return cipherTextBase64;

        byte[] key = Convert.FromBase64String(_secretKey);
        byte[] encryptedData = Convert.FromBase64String(cipherTextBase64);

        byte[] nonce = new byte[NonceSize];
        byte[] tag = new byte[TagSize];
        byte[] cipherBytes = new byte[encryptedData.Length - NonceSize - TagSize];

        Buffer.BlockCopy(encryptedData, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(encryptedData, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(encryptedData, NonceSize + TagSize, cipherBytes, 0, cipherBytes.Length);

        byte[] plainBytes = new byte[cipherBytes.Length];

        using (var aesGcm = new AesGcm(key, TagSize))
        {
            aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }

    public string GenerateMaskedAccountNumber(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber)) return string.Empty;
        if (accountNumber.Length <= 4) return accountNumber;

        string last4Digits = accountNumber.Substring(accountNumber.Length - 4);
        return new string('*', 6) + last4Digits;
    }
}