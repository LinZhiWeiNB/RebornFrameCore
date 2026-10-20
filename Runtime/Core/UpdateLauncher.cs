using System.IO;
using UnityEngine;

namespace  RebornFrame.Core
{
    public class UpdateLauncher
    {
        public virtual void Awake()
        {
            
        }

        public virtual void PullUpdate()
        {
            
        }

        public virtual string GetABRootPath(string assetsID)
        {
            return Path.Combine(Application.streamingAssetsPath, "HotUpdate") + "/";
        }
    }
}