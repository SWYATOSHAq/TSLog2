using System.Text.Json.Serialization;

namespace TSLog.Links
{
    public class User
    {
        public int Id { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public string Password { get; set; }
    }
}