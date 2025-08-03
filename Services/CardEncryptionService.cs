using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PopularBookstore.Services
{
    public class CardEncryptionService
    {
        private readonly string _encryptionKey;

        public CardEncryptionService(IConfiguration configuration)
        {
            _encryptionKey = configuration["CardEncryption:Key"] ?? "YourSecretKeyHere12345678901234567890";
        }

        public string EncryptCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber)) return string.Empty;

            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(cardNumber);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public string DecryptCardNumber(string encryptedCardNumber)
        {
            if (string.IsNullOrEmpty(encryptedCardNumber)) return string.Empty;

            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(encryptedCardNumber);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public string GetLastFourDigits(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return string.Empty;

            return cardNumber.Substring(cardNumber.Length - 4);
        }
    }
}