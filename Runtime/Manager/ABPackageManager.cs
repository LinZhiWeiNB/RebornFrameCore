using System.Collections;
using System.Collections.Generic;
using System.IO;
using RebornFrame.Core;
using RebornFrame.Utils;
using UnityEngine;

public class ABPackageManager : Singleton<ABPackageManager>
{
    private Dictionary<string, AssetBundle> loadedAssetBundels = new Dictionary<string, AssetBundle>();
    
    private ABPackageData _ABPackageData = null;
    private Dictionary<string,ABPackage> _ABPackage_Name_Infos = new Dictionary<string, ABPackage>();
    private Dictionary<string,ABPackage> _ABPackage_ID_Infos = new Dictionary<string, ABPackage>();

    public ABPackageManager()
    {
        LoadPackagedData();
    }
    
    public void LoadPackagedData()
    {
        //加载ABPackageInfo
        var dir = AppInstance.Instance.CustomUpdateLauncher.GetABRootPath("packagedata.json");
        var path = dir + "packagedata.json";
        string json = "";
#if !UNITY_EDITOR
        if (path.StartsWith(Application.streamingAssetsPath))
        {
            WWW dbPath = new WWW(path);
            while (!dbPath.isDone) { }
            json = dbPath.text;
            //去掉BOM头
            if (dbPath.bytes != null && dbPath.bytes[0] == 0xEF && dbPath.bytes[1] == 0xBB && dbPath.bytes[2] == 0xBF)
            {
                json = json.Substring(1);
            }
            
        }
        else
#endif
        
        {
            json = File.ReadAllText(path);
        }
        
        _ABPackageData = JsonUtility.FromJson<ABPackageData>(json);
        _ABPackage_Name_Infos.Clear();
        _ABPackage_ID_Infos.Clear();
        foreach (var abPackage in _ABPackageData.Packages)
        {
            _ABPackage_Name_Infos.Add(abPackage.PackageName,abPackage);
            _ABPackage_ID_Infos.Add(abPackage.PackageID,abPackage);
        }
    }
    
    public string LoadABDependencies(string packageName)
    {
        ABPackage package = null;
        if (!_ABPackage_ID_Infos.TryGetValue(packageName,out package))
        {
            packageName = AddExtension(packageName.ToLower());
            _ABPackage_Name_Infos.TryGetValue(packageName, out package);
        }
        
        if (package != null)
        {
            packageName = package.PackageID;
            for (var i = 0; i < package.dependencies.Count; i++)
            {
                var dep = package.dependencies[i];
                if (!loadedAssetBundels.ContainsKey(dep))
                {
                    var dir = AppInstance.Instance.CustomUpdateLauncher.GetABRootPath(dep);
                    var path = dir + dep;
                    var pack = AssetBundle.LoadFromFile(path);
                    loadedAssetBundels.Add(dep,pack);
                }
            }
        }
        
        return packageName;
    }
    
    public AssetBundle LoadAssetBundle(string loadPackage)
    {
        loadPackage = LoadABDependencies(loadPackage);
        if (!loadedAssetBundels.ContainsKey(loadPackage))
        {
            var dir = AppInstance.Instance.CustomUpdateLauncher.GetABRootPath(loadPackage);
            var path = dir + loadPackage;
            
            var pack = AssetBundle.LoadFromFile(path);
            loadedAssetBundels.Add(loadPackage,pack);
        }

        return loadedAssetBundels[loadPackage];
    }
    
    
    public T LoadAssetsFromABPath<T>(string path) where T : Object
    {
        var assetName = Path.GetFileNameWithoutExtension(path);
        var extends = Path.GetExtension(path);
        var loadPackage = path.Substring(0, path.Length - assetName.Length - extends.Length - 1).ToLower().Replace("/","_");
        var fullPackageName = AddExtension(loadPackage);
        if (_ABPackage_Name_Infos.ContainsKey(fullPackageName))
        {
            var packageInfo = _ABPackage_Name_Infos[fullPackageName];
            var index = packageInfo.additionalFiles.IndexOf("Assets/" + path);
            if (index >= 0)
            {
                loadPackage = packageInfo.additionalPackages[index];
            }
        }
        
        return LoadAssetBundle(loadPackage).LoadAsset<T>(assetName);
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="packageName"></param>
    /// <param name="assetName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public AssetBundleRequest LoadAssetsFromABPathAsync<T>(string path) where T : Object
    {
        var assetName = Path.GetFileNameWithoutExtension(path);
        var extends = Path.GetExtension(path);
        var loadPackage = path.Substring(0, path.Length - assetName.Length - extends.Length - 1).ToLower().Replace("/","_");
        var fullPackageName = AddExtension(loadPackage);
        if (_ABPackage_Name_Infos.ContainsKey(fullPackageName))
        {
            var packageInfo = _ABPackage_Name_Infos[fullPackageName];
            var index = packageInfo.additionalFiles.IndexOf("Assets/" + path);
            if (index >= 0)
            {
                loadPackage = packageInfo.additionalPackages[index];
            }
        }
        
        return LoadAssetBundle(loadPackage).LoadAssetAsync<T>(assetName);
    }
    
    
    public void UnLoadABPackage(string packageName)
    {
        packageName = AddExtension(packageName.ToLower());

        if (_ABPackage_Name_Infos.ContainsKey(packageName))
        {
            packageName = _ABPackage_Name_Infos[packageName].PackageID;
        }
        
        List<string> removeList = new List<string>();
        foreach (var package in loadedAssetBundels.Keys)
        {
            if (package.StartsWith($"{packageName}_") || package == AddExtension(packageName))
            {
                removeList.Add(package);
                loadedAssetBundels[package].Unload(true);
            }
        }
        for (var i = 0; i < removeList.Count; i++)
        {
            loadedAssetBundels.Remove(removeList[i]);
        }
    }
    
    public void UnLoadAllABPackage()
    {
        foreach (var package in loadedAssetBundels.Keys)
        {
            loadedAssetBundels[package].Unload(true);
        }
        loadedAssetBundels.Clear();
    }
    
    
    private string AddExtension(string loadPackage)
    {
#if UNITY_IPHONE
        loadPackage += "ios";
#elif UNITY_ANDROID
        loadPackage += "android";
#elif UNITY_STANDALONE_WIN
        loadPackage += "windows";
#elif UNITY_STANDALONE_OSX
        loadPackage += "mac";
#endif
        return loadPackage;
    }
}
