using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Links.JITLimport.Interfaces;

public interface IJsonParsingService
{
    void ParseJsonFiles(string directoryPath);
}
