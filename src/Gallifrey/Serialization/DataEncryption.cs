using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Gallifrey.Serialization
{
    internal static class DataEncryption
    {
        internal static string Encrypt(string plainText, string passPhrase)
        {
            var vector = GetSha256Hash(Guid.NewGuid().ToString()).Substring(0, 16).Replace("|", "~");
            var encrypted = Encrypt(plainText, passPhrase, vector);
            return $"V1|{vector}|{encrypted}";
        }

        internal static string Decrypt(string cipherText, string passPhrase)
        {
            var cipherParts = cipherText.Split(new[] { '|' }, 3);

            if (cipherParts.Length == 3 && cipherParts[0] == "V1")
            {
                return Decrypt(cipherParts[2], passPhrase, cipherParts[1]);
            }
            else
            {
                //Legacy handling
                return Decrypt(cipherText, "WOq2kKSbvHTcKp9e", "pId6i1bN1aCVTaHN");
            }
        }

        private static string Encrypt(string plainText, string passPhrase, string vector)
        {
            var initVectorBytes = Encoding.UTF8.GetBytes(vector);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new PasswordDeriveBytes(passPhrase, null);
            var keyBytes = password.GetBytes(32);
            var symmetricKey = new AesCryptoServiceProvider();
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

        private static string Decrypt(string cipherText, string passPhrase, string vector)
        {
            var initVectorBytes = Encoding.ASCII.GetBytes(vector);
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var password = new PasswordDeriveBytes(passPhrase, null);
            var keyBytes = password.GetBytes(32);
            var symmetricKey = new AesCryptoServiceProvider();
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
