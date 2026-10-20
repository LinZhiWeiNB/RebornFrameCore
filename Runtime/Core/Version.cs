using System;
using System.Collections.Generic;

namespace RebornFrame.Core
{
    [Serializable]
    public class FileMD5
    {
        public string AppVersion;
        public string ManiFestPackageName;
        public List<string> Files = new List<string>();
        public List<string> MD5 = new List<string>();
    }
}