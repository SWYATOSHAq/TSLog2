namespace TSLog.Links.MobileAppForBoxScan;
/// <summary>
/// Общая функциональность. Может быть использована в различных местах приложения.
/// </summary>
public class Common
{
    /// <summary>
    /// Версия приложения (увеличивать перед каждым выпуском).
    /// </summary>
    public static readonly string Version = "25.2.9.1";
    private const int SsccLength = 20;
    /// <summary>
    /// Проверяет SSCC на корректность.
    /// </summary>
    /// <param name="code">Входной SSCC.</param>
    /// <param name="sscc">Выходной SSCC (обрезаны концевые пробелы).</param>
    /// <returns>Признак корректность SSCC.</returns>
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
    /// Проверяет набор SSCC на корректность и на уникальность (в пределах набора).
    /// </summary>
    /// <param name="codes">Входной набор SSCC.</param>
    /// <param name="ssccs">Выходной набор SSCC (обрезаны концевые пробелы).</param>
    /// <returns>Признак корректность набора SSCC.</returns>
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
