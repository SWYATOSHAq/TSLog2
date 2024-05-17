using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Links.BLCtobacco.Interfaces;

public interface IHttpClientService
{
    Task<string> SendGetRequestAsync(string url);
    Task SendPostRequestAsync(string url, object data);
}
