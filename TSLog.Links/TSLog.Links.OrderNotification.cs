using System;

namespace TSLog.Links
{
    public class OrderNotofocation : Message
    {
        public OrderNotofocation(long id, char kind, string type, byte version, string body = null)
            : base(id, kind, type, version, body)
        { }
    }
}
