using System.Runtime.InteropServices;
using System.Security;

namespace TSLog.Common
{
    public static class Utils 
    {
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
    }
}
