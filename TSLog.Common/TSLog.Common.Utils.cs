using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TSLog.Common
{
    public static class Utils
    {

        public static string PrepXML(string a)
        {
            return a.Replace("><", "}{").Replace(">", ">\n\t").Replace("<", "\n<").Replace("}{", ">\n<");
        }

        public static string SecureStringToString(SecureString value)
        {
            var bstr = Marshal.SecureStringToBSTR(value);
            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

        private const string PathRegEx = @"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^/:*?<>""|]*))+)$";

        public static string ToSql(string value)
        {
            return !string.IsNullOrWhiteSpace(value) ? string.Format("'{0}'", value) : "null";
        }

        public static string ToSql(int value)
        {
            return value > 0 ? value.ToString() : "null";
        }

        public static string ToSql(long value)
        {
            return value > 0 ? value.ToString() : "null";
        }

        public static string ToSql(double value)
        {
            return value.ToString().Replace(',', '.');
        }

        public static string ToSql(DateTime value)
        {
            return value != DateTime.MinValue ? string.Format("'{0}'", value.ToString("dd.MM.yyyy")) : "null";
        }

        public static string ToSqlXmlText(string text)
        {
            return text == null ? null : text.Trim().Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", " ");
        }

        public static bool ValidFilePath(string path)
        {
            return (new Regex(PathRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase)).IsMatch(path);
        }

        public static string ClearNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return string.Empty;
            var stringBuilder = new StringBuilder(string.Empty);
            foreach (var symbol in number)
                if (char.IsLetterOrDigit(symbol))
                    stringBuilder.Append(symbol);
            var result = stringBuilder.ToString();
            return !string.IsNullOrWhiteSpace(result) ? result : number;
        }

        // Шифрование
        public static string Encrypt(string plainText, string password, string salt = "Kosher", string hashAlgorithm = "SHA1",
            int passwordIterations = 2, string initialVector = "OFRna73m*aze01xY", int keySize = 256)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] initialVectorBytes = Encoding.ASCII.GetBytes(initialVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(salt);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            var derivedPassword = new PasswordDeriveBytes(password, saltValueBytes, hashAlgorithm, passwordIterations);
            var keyBytes = derivedPassword.GetBytes(keySize / 8);
            var symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;

            byte[] cipherTextBytes = null;
            using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, initialVectorBytes))
            {
                using (var memStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        cipherTextBytes = memStream.ToArray();
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }
            symmetricKey.Clear();
            return Convert.ToBase64String(cipherTextBytes);
        }

        // Дешифрование
        public static string Decrypt(string cipherText, string password, string salt = "Kosher", string hashAlgorithm = "SHA1",
            int passwordIterations = 2, string initialVector = "OFRna73m*aze01xY", int keySize = 256)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            var initialVectorBytes = Encoding.ASCII.GetBytes(initialVector);
            var saltValueBytes = Encoding.ASCII.GetBytes(salt);
            var cipherTextBytes = Convert.FromBase64String(cipherText);

            var derivedPassword = new PasswordDeriveBytes(password, saltValueBytes, hashAlgorithm, passwordIterations);
            var keyBytes = derivedPassword.GetBytes(keySize / 8);

            var symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;

            var plainTextBytes = new byte[cipherTextBytes.Length];
            int byteCount = 0;
            using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, initialVectorBytes))
            {
                using (var memStream = new MemoryStream(cipherTextBytes))
                {
                    using (var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read))
                    {
                        byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }
            symmetricKey.Clear();
            return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
        }

        // Вычисление хэша md5
        public static string CalculateMd5Hash(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            var stringBuilder = new StringBuilder();
            foreach (var b in hash)
                stringBuilder.Append(b.ToString("X2"));
            return stringBuilder.ToString();
        }

        // Формирует адрес из элементов
        public static string FormAddress(string[] elements)
        {
            var address = string.Empty;
            foreach (var element in elements)
                if (!string.IsNullOrWhiteSpace(element))
                {
                    if (!string.IsNullOrWhiteSpace(address))
                        address = string.Format("{0}, ", address);
                    address = string.Format("{0}{1}", address, element);
                }
            return address;
        }

    }
}