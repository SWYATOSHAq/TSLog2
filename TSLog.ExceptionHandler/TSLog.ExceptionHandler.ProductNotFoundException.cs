using System;

namespace TSLog.ExceptionHandler
{
    public class ProductNotFoundException : Exception
    {
        public string Id { get; private set; }
        public ProductNotFoundException(string id)
            : base($"Product not found by id = {id}")
        {
            Id = id;
        }
    }
}