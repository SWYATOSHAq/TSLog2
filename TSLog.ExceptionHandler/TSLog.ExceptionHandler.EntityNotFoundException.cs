using System;

namespace TSLog.ExceptionHandler;
public class EntityNotFoundException : Exception
{
    public string Name { get; private set; }
    public long Id { get; private set; }
    public string Code { get; private set; }
    public EntityNotFoundException(int id): base(string.Format($"Entity not found by id = {id}", id))
    {
        Id = id;
    }
    public EntityNotFoundException(string name, int id) : base($"{name} not found by id = {id}")
    {
        Name = name;
        Id = id;
    }
    public EntityNotFoundException(string code): base(string.Format($"Entity not found by code = {code}", code))
    {
        Code = code;
    }
    public EntityNotFoundException(string name, string code) : base($"{name} not found by code = {code}")
    {
        Name = name;
        Code = code;
    }
}
