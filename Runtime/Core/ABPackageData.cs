using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RebornFrame.Core
{
    [Serializable]
    public class ABPackageData
    {
        public List<ABPackage> Packages = new List<ABPackage>();
        public string ManifestABName;
    }

    [Serializable]
    public class ABPackage
    {
        public string PackageID;
        public string PackageName;
        public List<string> additionalFiles = new List<string>();
        public List<string> additionalPackages = new List<string>();
        public List<string> dependencies = new List<string>();
    }
}