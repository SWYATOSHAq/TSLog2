using System;

namespace TSLog.ExceptionHandler
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(int id) : base(string.Format($"Entity not found by id = {id}", id))
        { }
        public EntityNotFoundException(string code) : base(string.Format($"Entity not found by code = {code}", code))
        { }
    }
}
