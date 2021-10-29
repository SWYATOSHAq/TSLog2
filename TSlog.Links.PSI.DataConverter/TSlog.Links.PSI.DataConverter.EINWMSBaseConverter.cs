using System;
using TSLog.BO;

namespace TSlog.Links.PSI.DataConverter
{
    public class EINWMSBaseConverter
    {
        public virtual string MessageName()
        {
            return "WMS";
        }

        public virtual string WMSAPIVersion()
        {
            return "1.9.3";
        }

        public static string PrepStrPar(string param, bool isUpper = false, string defaultValue = "")
        {
            var ret = string.IsNullOrWhiteSpace( param) ? defaultValue : param;
            return isUpper ? ret.ToUpper() : ret;
        }

        public static string PrepStrPar(int param, int defaultValue = 0)
        {
            var ret = param < 0 ? defaultValue : param;
            return ret.ToString();
        }

        public static string PrepStrPar(double param, double defaultValue = 0)
        {
            var ret = param < 0 ? defaultValue : param;
            return ret.ToString();
        }

        public static string TruncateLongString(string str, int maxLength, int start = 0)
        {
            string ret = "";
            if (string.IsNullOrEmpty(str)) ret = "";
            else
            {
                if (start > str.Length)
                    ret = "";
                else
                    ret = str.Substring(start, Math.Min(str.Length, maxLength));
            }


            return ret; 
        }

        public static string TranslateMeasurmentUnit(string unit)
        {
            // TODO read this data from file
            string ret = "";
            switch (unit)
            {
                case "мм":
                    ret = "MM";
                    break;

                case "см":
                    ret = "CM";
                    break;

                case "cm":
                    ret = "CM";
                    break;

                case "CM":
                    ret = "CM";
                    break;

                case "M":
                    ret = "M";
                    break;

                case "пог. м":
                    ret = "M";
                    break;

                case "м":
                    ret = "M";
                    break;

                case "мм3":
                    ret = "MM3";
                    break;

                case "СМ3":
                    ret = "CM3";
                    break;

                case "CM3":
                    ret = "CM3";
                    break;

                case "см3":
                    ret = "CM3";
                    break;

                case "дм3":
                    ret = "DM3";
                    break;

                case "м3":
                    ret = "M3";
                    break;

                case "г":
                    ret = "G";
                    break;

                case "G":
                    ret = "G";
                    break;

                case "KG":
                    ret = "KG";
                    break;

                case "кг":
                    ret = "KG";
                    break;

                case "компл":
                    ret = "SET";
                    break;

                case "упак":
                    ret = "SET";
                    break;

                case "шт.":
                    ret = "ST";
                    break;

                case "шт":
                    ret = "ST";
                    break;

                case "кор.":
                    ret = "ST";
                    break;

                case "кор":
                    ret = "ST";
                    break;

                case "пара":
                    ret = "ST";
                    break;

                case "бут":
                    ret = "ST";
                    break;

                case "уп.":
                    ret = "ST";
                    break;

                case "ST":
                    ret = "ST";
                    break;

                case "st":
                    ret = "ST";
                    break;

                default:
                    ret = "ST"; // huh
                    break;
            }

            return ret;
        }

        public static string GetDefaultMeasurmentUnit(LegalEntity client)
        {
            // TODO read this data from file
            string ret = "";

            switch (client.Code)
            {
                case "0002":
                    ret = "шт";
                    break;

                case "0007":
                    ret = "г";
                    break;

                default:
                    ret = "шт";
                    break;
            }

            return ret;
        }
    }
}