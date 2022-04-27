using System;

namespace TSLog.ExceptionHandler
{
    public class EmployeeNotFoundException : Exception
    {
        public int Id { get; private set; } = -1;
        public string FieldName { get; private set; } = string.Empty;
        public string FieldValue { get; private set; } = string.Empty;
        public EmployeeNotFoundException(int id)
            : base($"Employee not found by id = {id}")
        {
            Id = id;
        }
        public EmployeeNotFoundException(string fieldName, string fieldValue)
            : base($"Employee not found by {fieldName} = {fieldValue}")
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }
    }
}
