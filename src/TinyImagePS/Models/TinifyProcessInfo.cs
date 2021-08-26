using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace TinyImagePS.Models
{
    public class TinifyProcessInfo
    {
        public TinifyProcessInfo(TinifyResponse response, FileSystemInfo sourceFileInfo)
        {
            Input = response.Input;
            Output = response.Output;
            Source = sourceFileInfo;
        }

        public FileSystemInfo Source { get; set; } 
        public TinifyResponseInput Input { get; set; }
        public TinifyResponseOutput Output { get; set; }
    }
}
