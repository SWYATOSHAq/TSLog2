using System;

namespace TSLog.ExceptionHandler
{
    public class EmployeeNotFoundException : Exception
    {
        public int Id { get; private set; }
        public EmployeeNotFoundException(int id)
            : base($"Employee not found by id = {id}")
        {
            Id = id;
        }
    }
}
