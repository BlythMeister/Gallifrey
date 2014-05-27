using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Gallifrey.Serialization
{
    internal static class DataEncryption
    {
        //TODO: Maybe these shouldn't be stored in plain code?
        //TODO: Though, this is to make hacking the app settings a pain, rather than pure security.
        private const string PassPhrase = "WOq2kKSbvHTcKp9e";
        private const string InitVector = "pId6i1bN1aCVTaHN";
        private const int Keysize = 256;

        internal static string Encrypt(string plainText)
        {
            var initVectorBytes = Encoding.UTF8.GetBytes(InitVector);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new PasswordDeriveBytes(PassPhrase, null);
            var keyBytes = password.GetBytes(Keysize / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            var cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        internal static string Decrypt(string cipherText)
        {
            var initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var password = new PasswordDeriveBytes(PassPhrase, null);
            var keyBytes = password.GetBytes(Keysize / 8);
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };
            var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes = new byte[cipherTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
    }
}
