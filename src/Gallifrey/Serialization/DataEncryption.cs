using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Gallifrey.Serialization
{
    internal static class DataEncryption
    {
        internal static string EncryptCaseInsensitive(string plainText, string passPhrase)
        {
            var vector = GetSha256Hash(Guid.NewGuid().ToString()).Substring(0, 16).Replace("|", "~");
            var encrypted = Encrypt(plainText, passPhrase.ToLower(), vector);
            return $"V2|CI|{vector}|{encrypted}";
        }

        internal static string Decrypt(string cipherText, string passPhrase)
        {
            var cipherParts = cipherText.Split(new[] { '|' }, 2);

            if (cipherParts[0] == "V1")
            {
                cipherParts = cipherText.Split(new[] { '|' }, 3);
                return Decrypt(cipherParts[2], passPhrase, cipherParts[1]);
            }

            if (cipherParts[0] == "V2")
            {
                cipherParts = cipherText.Split(new[] { '|' }, 4);

                if (cipherParts[1] == "CI")
                {
                    return Decrypt(cipherParts[3], passPhrase.ToLower(), cipherParts[2]);
                }

                if (cipherParts[1] == "CS")
                {
                    return Decrypt(cipherParts[3], passPhrase, cipherParts[2]);
                }
            }

            return string.Empty;
        }

        private static string Encrypt(string plainText, string passPhrase, string vector)
        {
            var initVectorBytes = Encoding.UTF8.GetBytes(vector);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var keyBytes = new Rfc2898DeriveBytes(passPhrase, initVectorBytes).GetBytes(32);
            var encryptor = new AesCryptoServiceProvider().CreateEncryptor(keyBytes, initVectorBytes);

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

        private static string Decrypt(string cipherText, string passPhrase, string vector)
        {
            var initVectorBytes = Encoding.ASCII.GetBytes(vector);
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var keyBytes = new Rfc2898DeriveBytes(passPhrase, initVectorBytes).GetBytes(32);
            var decryptor = new AesCryptoServiceProvider().CreateDecryptor(keyBytes, initVectorBytes);

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
            var hashString = new SHA256Managed();
            var hash = hashString.ComputeHash(bytes);
            return hash.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
        }
    }
}
