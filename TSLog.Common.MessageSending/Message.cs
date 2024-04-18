using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Common.MessageSending;

public class Message
{
    public List<string> To { get; set; } = new List<string>();
    public string Subject { get; set; }
    public string Body { get; set; }
}
