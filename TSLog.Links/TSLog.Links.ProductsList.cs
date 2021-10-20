namespace TSLog.Links
{
    public class ProductsList : Message
    {
        public ProductsList(long id, char kind, string type, byte version, string body = null) : base(id, kind, type, version, body)
        {
            //
        }
    }
}
