using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AutoServiss.Helpers
{
    public class PasswordValidator
    {
        #region Password Validate

        public static string Validate(string password)
        {
            if (password == null)
            {
                return "nav norādīta parole";
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                return "Minimālais paroles garums ir 6 simboli";
            }

            if (password.All(IsLetterOrDigit))
            {
                return "Parolē jābūt vismaz 1 speciālajam simbolam";
            }

            if (!password.Any(IsDigit))
            {
                return "Parolē jābūt vismaz 1 ciparam";
            }

            if (!password.Any(IsLower))
            {
                return "Parolē jābūt vismaz 1 mazajam burtam";
            }

            //if (!password.Any(IsUpper))
            //{
            //    return"Parolē jābūt vismaz 1 lielajam burtam";
            //}

            return "OK";
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsLower(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        private static bool IsUpper(char c)
        {
            return c >= 'A' && c <= 'Z';
        }

        private static bool IsLetterOrDigit(char c)
        {
            return IsUpper(c) || IsLower(c) || IsDigit(c);
        }

        #endregion

        #region Password Encrypt/Decrypt

        public static string Encrypt(string clearText, string encryptionKey)
        {
            var clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText, string encryptionKey)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        #endregion
    }
}