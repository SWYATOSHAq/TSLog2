using System.Text.Json;

namespace TSLog.Common.Dto
{
    public class ErrorWebDto
    {
        public int StatusCode { get; set; }

        public string Message { get; set; }

        //public override string ToString()
        //{
        //    return JsonSerializer.Serialize(this);
        //}
    }
}