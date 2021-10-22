using System.Collections.Generic;
using TSLog.BO;

namespace TSLog.Links
{
    public class ClientsList : Message
    {
        public ClientsList(long id, char kind, string type, byte version, string body = null) : base(id, kind, type, version, body)
        {
            clients = new List<LegalEntity>();
        }

        public List<LegalEntity> clients { get; set; }
    }
}
