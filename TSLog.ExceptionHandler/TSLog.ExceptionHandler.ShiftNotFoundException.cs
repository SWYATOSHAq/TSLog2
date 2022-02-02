using System;

namespace TSLog.ExceptionHandler
{
    public class ShiftNotFoundException : Exception
    {
        public int Id { get; private set; }
        public ShiftNotFoundException(int id)
            : base($"Yard operation not found by id = {id}")
        {
            Id = id;
        }
    }
}
