using System;
using System.IO;
using System.Security.Cryptography;

namespace EtbSomalia.Extensions
{
    public class CrytoUtilsExtensions {
        private readonly byte[] KEY_64 = { 42, 16, 93, 156, 78, 4, 218, 32 };
        private readonly byte[] IV_64 = { 55, 103, 246, 79, 36, 99, 167, 3 };

        private readonly byte[] KEY_192 = { 42, 16, 93, 156, 78, 4, 218, 32, 2, 94, 11, 204, 119, 35, 184, 197 };
        private readonly byte[] IV_192 = { 55, 103, 246, 79, 36, 99, 167, 3, 42, 5, 62, 83, 184, 7, 209, 13, 145, 23, 200, 58, 173, 10, 121, 222 };

        public string Encrypt(string value) {
            if (string.IsNullOrEmpty(value))
                return null;

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateEncryptor(KEY_64, IV_64), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cs);

            sw.Write(value);
            sw.Flush();
            cs.FlushFinalBlock();
            ms.Flush();

            return Convert.ToBase64String(ms.GetBuffer(), 0, ms.Length.GetHashCode());

        }

        public string Decrypt(string value) {
            if (string.IsNullOrEmpty(value))
                return null;

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();

            byte[] buffer = Convert.FromBase64String(value);
            MemoryStream ms = new MemoryStream(buffer);
            CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateDecryptor(KEY_64, IV_64), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

    }
}
