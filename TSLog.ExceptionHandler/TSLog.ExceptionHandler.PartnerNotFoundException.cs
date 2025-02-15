using System;

namespace TSLog.ExceptionHandler;

public class PartnerNotFoundException : Exception
{
    public string Id { get; private set; }
    public PartnerNotFoundException(string id)
        : base($"Partner not found by id = {id}")
    {
        Id = id;
    }
}
