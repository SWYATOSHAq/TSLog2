using System;

namespace TSLog.ExceptionHandler
{
    public class ProductNotFoundException : Exception
    {
        public string ProductCode { get; private set; }
    }
}