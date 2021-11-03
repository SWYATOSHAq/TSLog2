using System;

namespace TSLog.ExceptionHandler
{
    public class LegalEntityNotFoundException : Exception
    {
        public string LegalEntityCode { get; private set; }
    }
}