using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace RebornFrame.Core
{
    public class ABExportConfig : ScriptableObject
    {
        public List<SingleABConfig> Configs = new List<SingleABConfig>()
        {
            new SingleABConfig(){Name = "配置文件",Path = "Arts/Configs",MatchRegex = @"^(?!.+\.meta$).+$",NeedExport = true},
            new SingleABConfig(){Name = "FGUI",Path = "Arts/UI",MatchRegex = @"^(?!.+\.meta$).+$",NeedExport = true},
            new SingleABConfig(){Name = "贴图",Path = "Arts/Textures/Export",MatchRegex = "/*.(png|jpg|tga|jpeg)$",NeedExport = false},
            new SingleABConfig(){Name = "材质",Path = "Arts/Materials/Export",MatchRegex = "/*.(mat)$",NeedExport = false},
            new SingleABConfig(){Name = "模型",Path = "Arts/Meshes/Export",MatchRegex = "/*.(fbx|obj)$",NeedExport = false},
            new SingleABConfig(){Name = "Shaders",Path = "Arts/Shaders/Export",MatchRegex = "/*.(shader|shadergraph)$",NeedExport = false},
            new SingleABConfig(){Name = "场景",Path = "Arts/Scenes/Export",MatchRegex = "/*.(unity)$",NeedExport = false},
            new SingleABConfig(){Name = "动画",Path = "Arts/Animations/Export",MatchRegex = "/*.(controller|playable)$",NeedExport = false},
            new SingleABConfig(){Name = "TimeLine",Path = "Arts/Timeline/Export",MatchRegex = "/*.(controller|playable)$",NeedExport = false},
            new SingleABConfig(){Name = "音频",Path = "Arts/Audios/Export",MatchRegex = "/*.(mp3|wav|ogg)$",NeedExport = false},
            new SingleABConfig(){Name = "特效",Path = "Arts/Effects/Export",MatchRegex = "/*.(prefab|meta)$",NeedExport = false},
            new SingleABConfig(){Name = "预制体",Path = "Arts/Prefabs/Export",MatchRegex = "/*.(prefab|meta)$",NeedExport = false},
            
        };
        public List<string> AOTDLLList = new List<string>();
        public string AppID = "";
        public string ABRootPath = "StreamingAssets/HotUpdate";
        public string ComparePath = "../Export/HotUpdateDifference";
        public string ServerRelaseVersionPath = "http://carinno.icu:666/HotUpdate/";
        public string ServerRelaseMD5Path = "http://carinno.icu:666/HotUpdate/";
        public string ServerDevVersionPath = "http://carinno.icu:666/HotUpdate/";
        public string ServerDevMD5Path = "http://carinno.icu:666/HotUpdate/";
        public string ServerTestVersionPath = "http://carinno.icu:666/HotUpdate/";
        public string ServerTestMD5Path = "http://carinno.icu:666/HotUpdate/";
        public string DLLPath = "../HybridCLRData/HotUpdateDlls/Android/HotUpdate.dll";
    }

    [Serializable]
    public class SingleABConfig
    {
        public string Name;
        public string Path;
        public string MatchRegex;
        public bool NeedExport;
    }

    public class VersionConfirmDialog : ScriptableWizard
    {
        public string 新版本号;

        public void OnWizardCreate()
        {
            ABToolEditor.ContinueCompare?.Invoke(新版本号);
        }
    }
}