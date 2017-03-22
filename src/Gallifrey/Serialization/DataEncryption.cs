using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Gallifrey.Settings;

namespace Gallifrey.Serialization
{
    internal static class DataEncryption
    {
        internal static string Encrypt(string plainText)
        {
            var initVectorBytes = Encoding.UTF8.GetBytes(ConfigKeys.DataEncryptionInitVector);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new PasswordDeriveBytes(ConfigKeys.DataEncryptionPassPhrase, null);
            var keyBytes = password.GetBytes(ConfigKeys.DataEncryptionKeysize / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    var cipherTextBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(cipherTextBytes);
                }
            }
        }

        internal static string Decrypt(string cipherText)
        {
            var initVectorBytes = Encoding.ASCII.GetBytes(ConfigKeys.DataEncryptionInitVector);
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var password = new PasswordDeriveBytes(ConfigKeys.DataEncryptionPassPhrase, null);
            var keyBytes = password.GetBytes(ConfigKeys.DataEncryptionKeysize / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

            using (var memoryStream = new MemoryStream(cipherTextBytes))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    var plainTextBytes = new byte[cipherTextBytes.Length];
                    var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                }
            }
        }

        internal static string GetSha256Hash(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var hashstring = new SHA256Managed();
            var hash = hashstring.ComputeHash(bytes);
            return hash.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
        }
    }
}
