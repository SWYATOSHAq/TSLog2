using System;

namespace TSLog.ExceptionHandler
{
    public class OperationNotFoundException : Exception
    {
        public long Id { get; private set; }
        public OperationNotFoundException(long id) : base(string.Format($"Yard operation not found by id = {id}", id))
        {
            Id = id;
        }
    }
}
