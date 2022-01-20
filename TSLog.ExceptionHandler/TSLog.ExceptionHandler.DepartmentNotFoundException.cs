using System;

namespace TSLog.ExceptionHandler
{
    public class DepartmentNotFoundException : Exception
    {
        public int Id { get; private set; }
        public string Code { get; private set; }
        public DepartmentNotFoundException(int id)
            : base($"Department not found by id = {id}")
        {
            Id = id;
        }
        public DepartmentNotFoundException(string code)
            : base($"Department not found by code = {code}")
        {
            Code = code;
        }
    }
}