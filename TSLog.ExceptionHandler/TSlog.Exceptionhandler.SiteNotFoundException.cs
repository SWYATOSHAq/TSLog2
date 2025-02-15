using System;

namespace TSLog.ExceptionHandler;
public class SiteNotFoundException : Exception
{
    public short Id { get; private set; }
    public SiteNotFoundException(short id)
        : base($"Site not found by id = {id}")
    {
        Id = id;
    }
}
