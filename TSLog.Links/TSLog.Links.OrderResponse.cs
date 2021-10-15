using TSLog.BO;

namespace TSLog.Links
{
    public class OrderResponse : Message
    {
        private Order _order = null;
        public OrderResponse(long id, char kind, string type, byte version, string body = null)
            : base(id, kind, type, version, body)
        { }
        public string Data { get; set; }
    }
}
