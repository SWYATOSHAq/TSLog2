using System;

namespace TSLog.ExceptionHandler;
public class LegalEntityNotFoundException : Exception
{
    public string Id { get; private set; }
    public LegalEntityNotFoundException(string id)
        : base($"Legal entity not found by id = {id}")
    {
        Id = id;
    }
}