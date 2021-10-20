namespace TSLog.Links
{
    public class ClientsList : Message
    {
        public ClientsList(long id, char kind, string type, byte version, string body = null) : base(id, kind, type, version, body)
        {
            //
        }
    }
}
