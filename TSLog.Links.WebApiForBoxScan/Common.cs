namespace TSLog.Links.WebApiForBoxScan;

/// <summary>
/// Общая функциональность.
/// </summary>
public class Common
{
    public const string Version = "25.2.1.1";
    private const int SsccLength = 20;
    /// <summary>
    /// Проверка SSCC.
    /// </summary>
    /// <param name="code">Входной SSCC.</param>
    /// <param name="sscc">Выходной SSCC после проверки и обработки.</param>
    /// <returns>Результат проверки (успех или нет).</returns>
    public static bool CheckSscc(string code, out string sscc)
    {
        sscc = string.Empty;
        if (string.IsNullOrWhiteSpace(code))
            return false;
        var temp = code.Trim();
        if (temp.Length != SsccLength)
            return false;
        if (!temp.All(char.IsDigit))
            return false;

        sscc = temp;
        return true;
    }
    /// <summary>
    /// Проверка набора SSCC. Также проверяется уникальность в наборе.
    /// </summary>
    /// <param name="codes">Входной набор SSCC.</param>
    /// <param name="ssccs">Выходной набор SSCC после проверки и обработки.</param>
    /// <returns>Результат проверки (успех или нет),
    /// для успешного прохождения ВСЕ! SSCC должны пройти проверку.</returns>
    public static bool CheckSsccs(string[] codes, out ISet<string> ssccs)
    {
        ssccs = new HashSet<string>();
        var temp = new HashSet<string>();
        foreach (var code in codes)
        {
            if (!CheckSscc(code, out string sscc))
                return false;
            if (temp.Contains(sscc))
                return false;
            temp.Add(sscc);
        }
        ssccs = temp;
        return true;
    }
}
