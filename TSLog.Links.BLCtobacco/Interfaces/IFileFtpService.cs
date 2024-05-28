using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Links.BLCtobacco.Interfaces;

internal interface IFileFtpService
{
    void CreateInMessage();
    void CreateOutMessage();
    Task UploadFileToServerAsync();
}
