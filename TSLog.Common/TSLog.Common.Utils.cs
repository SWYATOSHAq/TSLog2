using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TSLog.Common
{
    /// <summary>
    /// Общий функционал.
    /// </summary>
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

        public static SecureString StringToSecureString(string value)
        {
            var result = new SecureString();
            foreach (var symbol in value)
                result.AppendChar(symbol);
            return result;
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
            return text?.Trim().Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", " ");
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

        /// <summary>
        /// Шифрование
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <param name="hashAlgorithm"></param>
        /// <param name="passwordIterations"></param>
        /// <param name="initialVector"></param>
        /// <param name="keySize"></param>
        /// <returns></returns>
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
            using var symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;

            byte[] cipherTextBytes = null;
            using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, initialVectorBytes))
            {
                using var memStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                cipherTextBytes = memStream.ToArray();
                memStream.Close();
                cryptoStream.Close();
            }
            symmetricKey.Clear();
            return Convert.ToBase64String(cipherTextBytes);
        }

        /// <summary>
        /// Дешифрование
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <param name="hashAlgorithm"></param>
        /// <param name="passwordIterations"></param>
        /// <param name="initialVector"></param>
        /// <param name="keySize"></param>
        /// <returns></returns>
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

            var symmetricKey = new RijndaelManaged
            {
                Mode = CipherMode.CBC
            };

            var plainTextBytes = new byte[cipherTextBytes.Length];
            int byteCount = 0;
            using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, initialVectorBytes))
            {
                using var memStream = new MemoryStream(cipherTextBytes);
                using var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read);
                byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memStream.Close();
                cryptoStream.Close();
            }
            symmetricKey.Clear();
            return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
        }

        /// <summary>
        /// Вычисление хэша md5
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CalculateMd5Hash(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            var stringBuilder = new StringBuilder();
            foreach (var b in hash)
                stringBuilder.Append(b.ToString("X2"));
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Формирует адрес из элементов
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
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

        public static DateTime DbValueToDateTime(object value, DateTime? defaultValue = null)
        { 
            return value != DBNull.Value ? Convert.ToDateTime(value) : (defaultValue ?? DateTime.MinValue);
        }
        public static short DbValueToShort(object value, short? defaultValue = null)
        {
            return value != DBNull.Value ? Convert.ToInt16(value) : (defaultValue ?? short.MinValue);
        }
        public static int DbValueToInt(object value, int? defaultValue = null)
        {
            return value != DBNull.Value ? Convert.ToInt32(value) : (defaultValue ?? int.MinValue);
        }
        public static long DbValueToLong(object value, long? defaultValue = null)
        {
            return value != DBNull.Value ? Convert.ToInt64(value) : (defaultValue ?? long.MinValue);
        }
        public static double DbValueToDouble(object value, double? defaultValue = null)
        {
            return value != DBNull.Value ? Convert.ToDouble(value) : (defaultValue ?? double.MinValue);
        }
        public static string DbValueToString(object value, string defaultValue = null)
        {
            return value != DBNull.Value ? Convert.ToString(value) : (defaultValue ?? string.Empty);
        }

        /// <summary>
        /// Преобразует полное Ф. И. О. (Фамилия Имя Отчество) в краткое (Фамилия И. О.).
        /// </summary>
        /// <param name="fullName">Полное Ф. И. О.</param>
        /// <returns>Краткое Ф. И. О.</returns>
        public static string FullNameToShortName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;
            while (fullName.IndexOf("  ") > 0)
                fullName = fullName.Replace("  ", " ");
            fullName = fullName.Trim();

            var names = fullName.Split(' ');
            var shortName = names[0];
            for (int i = 1; i < names.Length; i++) 
                shortName = shortName + " " + names[i][0] + ".";
            return shortName;
        }
        /// <summary>
        /// Возвращает массив дней (дат), которые попадают в заданный промежуток (начало включено, окончание не включено).
        /// </summary>
        /// <param name="fromDate">Дата и время начала промежутка.</param>
        /// <param name="toDate">Дата и время окончания промежутка.</param>
        /// <returns>Массив дней (дат).</returns>
        public static DateTime[] GetDates(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
                return Array.Empty<DateTime>();
            var dates = new List<DateTime>();
            var tempDate = fromDate.Date;
            while (tempDate < toDate) 
            { 
                dates.Add(tempDate);
                tempDate.AddDays(1);
            }
            return dates.ToArray();
        }

        private static readonly string[] Formats = { "M/d/yyyy hh:mm:ss tt", "dd.MM.yyyy HH:mm:ss" };
        public static bool DateFromString(string dateString, out DateTime date, out string message)
        {
            message = string.Empty;
            if (!DateTime.TryParseExact(dateString, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                message = $"Wrong start date {dateString}!";
                return false;
            }
            return true;    
        }
    }
}
