namespace TSLog.Links.Sscc;

/// <summary>
/// Общая функциональность.
/// </summary>
public class Utils
{
    private const int SsccLength18 = 18;
    private const int SsccLength20 = 20;
    private const int SsccLength22 = 22;

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

        if (temp.Length == SsccLength18 || temp.Length == SsccLength20)
        {
            if (!temp.All(char.IsDigit))
                return false;
        }
        else if (temp.Length == SsccLength22)
        {
            if (temp[0] != '(' || temp[3] != ')')
                return false;
            if (!temp.Substring(1, 2).All(char.IsDigit) || !temp.Substring(4).All(char.IsDigit))
                return false;
        }
        else
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
