using System;

namespace TSLog.ExceptionHandler;
public class WarehouseActionNotFoundException : Exception
{
    public string Id { get; private set; }
    public string PartnerId { get; private set; }
    public string PartnerActionId { get; private set; }
    public WarehouseActionNotFoundException(string id) 
        : base($"Warehouse action not found by id = {id}")
    {
        Id = id;
    }
    public WarehouseActionNotFoundException(string partnerId, string partnerActionId)
        : base($"Warehouse action not found by partner id = {partnerId} and partner action id = {partnerActionId}")
    {
        PartnerId = partnerId;
        PartnerActionId = partnerActionId;
    }
}
