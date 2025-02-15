using System;

namespace TSLog.ExceptionHandler;

public class PositionNotFoundException : Exception
{
    public int Id { get; private set; }
    public string Code { get; private set; }
    public PositionNotFoundException(int id)
        : base($"Position not found by id = {id}")
    {
        Id = id;
    }
    public PositionNotFoundException(string code)
        : base($"Position not found by code = {code}")
    {
        Code = code;
    }
}
