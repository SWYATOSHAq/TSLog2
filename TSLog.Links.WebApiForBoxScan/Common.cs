namespace TSLog.Links.WebApiForBoxScan
{
    public class Common
    {
        private const int SsccLength = 20;
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
}
