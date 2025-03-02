using System;
using TSLog.Common.BO;
using TSLog.LegalEntities.BO;

namespace TSlog.Links.PSI.DataConverter;

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
        var ret = string.IsNullOrWhiteSpace(param) ? defaultValue : param;
        return isUpper ? ret.ToUpper() : ret;
    }

    public static string PrepStrPar(int param, int defaultValue = 0)
    {
        var ret = param < 0 ? defaultValue : param;
        return ret.ToString();
    }

    public static string PrepStrPar(MeasureUnit param, string defaultValue = "")
    {
        string ret;
        if (param == MeasureUnit.Null)
            ret = defaultValue;
        else
            ret = param.Code;
        //param < 0 ? defaultValue : param;
        return ret;
    }

    public static string PrepStrPar(OperationType param, string defaultValue = "")
    {
        var ret = param == OperationType.Undefined ? defaultValue : ((int)param).ToString();
        return ret.ToString();
    }

    public static string PrepStrPar(DateTime param, string defaultValue = "", string template = "dd.MM.yyyy HH:mm:ss")
    {
        var ret = param == DateTime.MinValue ? defaultValue : param.ToString(template);
        return ret;
    }

    public static string PrepStrPar(double param, double defaultValue = 0, bool retEmptyOnZero = false)
    {
        var retVal = param < 0 ? defaultValue : param;

        var ret = retVal.ToString();
        if (retVal == 0)
            ret = string.Empty;

        return ret;
    }

    public static string TruncateLongString(string str, int maxLength, int start = 0)
    {
        string ret;
        if (string.IsNullOrEmpty(str)) 
            ret = "";
        else
        {
            if (start > str.Length)
                ret = string.Empty;
            else
                ret = str.Substring(start, Math.Min(str.Length, maxLength));
        }

        return ret;
    }

    public static string TranslateMeasurmentUnit(string unit)
    {
        // TODO: read this data from file.
        string ret = unit switch
        {
            "мм" => "MM",
            "см" => "CM",
            "cm" => "CM",
            "CM" => "CM",
            "M" => "M",
            "пог. м" => "M",
            "м" => "M",
            "мм3" => "MM3",
            "СМ3" => "CM3",
            "CM3" => "CM3",
            "см3" => "CM3",
            "дм3" => "DM3",
            "м3" => "M3",
            "г" => "G",
            "G" => "G",
            "KG" => "KG",
            "кг" => "KG",
            "компл" => "SET",
            "упак" => "SET",
            "шт." => "ST",
            "шт" => "ST",
            "кор." => "ST",
            "кор" => "ST",
            "пара" => "ST",
            "бут" => "ST",
            "уп." => "ST",
            "ST" => "ST",
            "st" => "ST",
            "" => "ST",
            _ => unit, // Huh.
        };
        return ret;
    }

    public static string GetDefaultMeasurmentUnit(LegalEntity client)
    {
        // TODO: read this data from file.
        string ret = client.Code switch
        {
            "0002" => "шт",
            "0007" => "г",
            _ => "шт",
        };
        return ret;
    }
}
