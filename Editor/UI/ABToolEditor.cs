using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace RebornFrame.Core
{
    public class ABToolEditor : EditorWindow
    {
        [MenuItem("RebornFrame/ABToolEditor", false, 20)]
        public static void OpenWindow()
        {
            ABToolEditor wnd = GetWindow<ABToolEditor>();
            wnd.titleContent = new GUIContent("ABToolEditor");
        }
        
        public TextField _outPath;
        public TextField _comparePath;
        public TextField _serverVersionPath;
        public TextField _DLLPath;
        public TextField _serverMD5Path;
        public ScrollView _addtionalDLL;
        public ScrollView _exportPaths;
        public ABExportConfig _config;
        private const string _configPath = "Assets/Editor/ABExportConfig.asset";
        private Dictionary<string, SingleABContent> _singleABContents = new Dictionary<string, SingleABContent>();

        private string editorPath;
        private string serverVersionFilePath;
        private string serverMD5FilePath;
        public VisualElement view;

        public static Action<string> ContinueCompare;
        public static string versionFileName = "version.txt";
        public static string md5FileName = "filemd5.json";

        public List<AssetBundleBuild> Builds = new List<AssetBundleBuild>();
        public Dictionary<string,List<string>> File_BuildKey = new Dictionary<string, List<string>>();
        public void CreateGUI()
        {
            editorPath = Application.dataPath + "/" + "Editor";
            serverVersionFilePath = editorPath + "/" + "ServerVersion.txt";
            serverMD5FilePath = editorPath + "/" + "MD5.Json";
            ContinueCompare = null;

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            if (!Directory.Exists(editorPath))
                Directory.CreateDirectory(editorPath);

            _config = AssetDatabase.LoadAssetAtPath<ABExportConfig>(_configPath);
            if (_config == null)
            {
                _config = CreateInstance<ABExportConfig>();
                AssetDatabase.CreateAsset(_config, _configPath);
            }

            // Import UXML
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.reborn.reborncore/Editor/UI/ABToolEditor.uss");
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.reborn.reborncore/Editor/UI/ABToolEditor.uxml");
            view = visualTree.Instantiate();
            view.styleSheets.Add(styleSheet);
            root.Add(view);
            _outPath = view.Q<TextField>("OutPutPath");
            _serverVersionPath = view.Q<TextField>("ServerVersionPath");
            _comparePath = view.Q<TextField>("ComparePath");
            _DLLPath = view.Q<TextField>("DLLPath");
            _serverMD5Path = view.Q<TextField>("ServerMD5Path");
            _addtionalDLL = view.Q<ScrollView>("AddtionalDLL");
            var addDLLBtn = new Button();
            addDLLBtn.clicked += AddAdditionDLL;
            addDLLBtn.text = "增加一个DLL";
            _addtionalDLL.Add(addDLLBtn);
            _config.AOTDLLList.ForEach((dll) =>
            {
                var dllPath = new TextField();
                dllPath.value = dll;
                _addtionalDLL.Insert(_addtionalDLL.childCount - 1, dllPath);
            });
            
            
            _outPath.value = _config.ABRootPath;
            _comparePath.value = _config.ComparePath;
            _serverVersionPath.value = _config.ServerDevVersionPath;
            _DLLPath.value = _config.DLLPath;
            _serverMD5Path.value = _config.ServerDevMD5Path;


            _exportPaths = view.Q<ScrollView>("ScrollView"); 
            var addExportPathBtn = new Button();
            addExportPathBtn.clicked += AddExportPathBtn;
            addExportPathBtn.text = "增加一个导出路径";
            _exportPaths.Add(addExportPathBtn);
            _config.Configs.ForEach(AddExportPathBtn);

            view.Q<Button>("ClearBuild").clicked += ClearOutPathClick;
            view.Q<Button>("ExportAll").clicked += ExportAll;
            view.Q<Button>("Compare").clicked += Compare;
            view.Q<Button>("FirstVersion").clicked += FirstVersion;
            view.Q<Button>("DownloadServerVersion").clicked += DownloadServerVersion;
            view.Q<Button>("DownloadServerMD5").clicked += DownloadServerMD5;
            view.Q<Button>("CopyDll").clicked += CopyDLL;
            var BranchDropDown = view.Q<DropdownField>("BranchDropDown");
            BranchDropDown.choices = new List<string>();
            BranchDropDown.choices.Add("开发");
            BranchDropDown.choices.Add("测试");
            BranchDropDown.choices.Add("发布");
            BranchDropDown.value = "开发";
            BranchDropDown.RegisterCallback<ChangeEvent<string>>((change) =>
            {
                switch (change.newValue)
                {
                    case "开发":
                        _serverVersionPath.value = _config.ServerDevVersionPath;
                        _serverMD5Path.value = _config.ServerDevMD5Path;
                        break;
                    case "测试":
                        _serverVersionPath.value = _config.ServerTestVersionPath;
                        _serverMD5Path.value = _config.ServerTestMD5Path;
                        break;
                    case "发布":
                        _serverVersionPath.value = _config.ServerRelaseVersionPath;
                        _serverMD5Path.value = _config.ServerTestVersionPath;
                        break;
                }
            });
        }

        private void AddExportPathBtn()
        {
            SingleABContent singleABContent = new SingleABContent(this);
            _singleABContents.Add(singleABContent._name.value, singleABContent);
        }
        private void AddExportPathBtn(SingleABConfig config)
        {
            SingleABContent singleABContent = new SingleABContent(this);
            singleABContent._name.value = config.Name;
            singleABContent._filePath.value = config.Path;
            singleABContent._matchRegex.value = config.MatchRegex;
            singleABContent._exportToggle.value = config.NeedExport;
            _singleABContents.Add(singleABContent._name.value, singleABContent);
        }

        private void AddAdditionDLL()
        {
            _addtionalDLL.Insert(_addtionalDLL.childCount - 1, new TextField());
        }

        private void CopyDLL()
        {
            //检查是否有下载地址
            if (_DLLPath.value == "")
            {
                EditorUtility.DisplayDialog("请填DLL文件路径", "请填DLL文件路径", "确定");
                return;
            }
            var DllDir = Path.Combine(Application.dataPath, Path.GetDirectoryName(_DLLPath.value));
            if (!Directory.Exists(DllDir))
            {
                EditorUtility.DisplayDialog("DLL文件路径不存在", "请检查是否正确使用HybridCLR", "确定");
                return;
            }

            var path = Application.dataPath + "/" + _outPath.value;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //检查是否有dll文件
            if (_DLLPath.value != "")
            {
                File.Copy(Path.Combine(Application.dataPath,_DLLPath.value),
                    Path.Combine(path, Path.GetFileName(_DLLPath.value)), true);
            }
            
            //AddtionalDLL
            List<string> NotFoundDLL = new List<string>();
            for (var i = 0; i < _addtionalDLL.childCount - 1; i++)
            {
                if(_addtionalDLL[i] is TextField dllPath && dllPath.value != "")
                {
                    string filePath =
                        (dllPath.value.Contains("/") || dllPath.value.Contains("/"))
                            ? Path.Combine(Application.dataPath, dllPath.value)
                            : Path.Combine(DllDir, dllPath.value);
                    Debug.Log(filePath);
                    if (File.Exists(filePath))
                    {
                        File.Copy(filePath,
                            Path.Combine(path, Path.GetFileName(dllPath.value)), true);
                    }
                    else
                    {
                        NotFoundDLL.Add(dllPath.value);
                    }
                }
            }

            CaculateMD5();

            var versionPath = Application.dataPath + "/" + _outPath.value +
                              "/" + versionFileName;
            File.Create(versionPath).Close();
            
            SaveConfig();
            AssetDatabase.Refresh();
            //提示没找到的dll
            if (NotFoundDLL.Count > 0)
            {
                StringBuilder sb = new StringBuilder("以下DLL文件未找到：\r\n");
                foreach (var dll in NotFoundDLL)
                {
                    sb.Append(dll);
                    sb.Append("\r\n");
                }

                EditorUtility.DisplayDialog("复制完成", sb.ToString(), "确定");
            }else
                EditorUtility.DisplayDialog("复制完成", "复制完成", "确定");
        }

        private void DownloadServerVersion()
        {
            //检查是否有下载地址
            if (_serverVersionPath.value == "")
            {
                EditorUtility.DisplayDialog("请填写下载地址", "请填写下载地址", "确定");
                return;
            }

            if (File.Exists(serverVersionFilePath))
                File.Delete(serverVersionFilePath);

            HttpDownloadFile(_serverVersionPath.value, serverVersionFilePath);

            SaveConfig();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("下载完成", "下载完成", "确定");
        }


        private void DownloadServerMD5()
        {
            //检查是否有下载地址
            if (_serverMD5Path.value == "")
            {
                EditorUtility.DisplayDialog("请填写下载地址", "请填写下载地址", "确定");
                return;
            }

            if (File.Exists(serverMD5FilePath))
                File.Delete(serverMD5FilePath);

            HttpDownloadFile(_serverMD5Path.value, serverMD5FilePath);

            SaveConfig();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("下载完成", "下载完成", "确定");
        }

        public void HttpDownloadFile(string url, string path)
        {
            // 设置参数

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //发送请求并获取相应回应数据

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            //直到request.GetResponse()程序才开始向目标网页发送Post请求

            Stream responseStream = response.GetResponseStream();
            //创建本地文件写入流

            if (Directory.Exists(Path.GetDirectoryName(path)))
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }


            Stream stream = new FileStream(path, FileMode.Create);
            byte[] bArr = new byte[1024];

            int size = responseStream.Read(bArr, 0, (int)bArr.Length);

            while (size > 0)

            {
                stream.Write(bArr, 0, size);

                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }

            stream.Close();

            responseStream.Close();
        }

        public void SaveConfig()
        {
            var config = CreateInstance<ABExportConfig>();
            config.ABRootPath = _outPath.value;
            config.ServerDevVersionPath = _config.ServerDevVersionPath;
            config.ServerDevMD5Path = _config.ServerDevMD5Path;
            config.ServerTestVersionPath = _config.ServerTestVersionPath;
            config.ServerTestMD5Path = _config.ServerTestMD5Path;
            config.ServerRelaseVersionPath = _config.ServerRelaseVersionPath;
            config.ServerRelaseMD5Path = _config.ServerRelaseMD5Path;
            switch (view.Q<DropdownField>("BranchDropDown").value)
            {
                case "开发":
                    config.ServerDevVersionPath = _serverVersionPath.value;
                    config.ServerDevMD5Path = _serverMD5Path.value;
                    break;
                case "测试":
                    config.ServerTestVersionPath = _serverVersionPath.value;
                    config.ServerTestMD5Path = _serverMD5Path.value;
                    break;
                case "发布":
                    config.ServerRelaseVersionPath = _serverVersionPath.value;
                    config.ServerTestVersionPath = _serverMD5Path.value;
                    break;
            }

            config.ComparePath = _comparePath.value;
            config.DLLPath = _DLLPath.value;
            config.Configs.Clear();
            
            foreach (var singleAbContent in _singleABContents)
            {
                if(string.IsNullOrEmpty(singleAbContent.Value._name.value))
                    continue;
                
                config.Configs.Add(new SingleABConfig()
                {
                    Name = singleAbContent.Value._name.value,
                    MatchRegex = singleAbContent.Value._matchRegex.value,
                    Path = singleAbContent.Value._filePath.value,
                    NeedExport = singleAbContent.Value._exportToggle.value
                });
            }
            
            //AddtionalDLL
            config.AOTDLLList.Clear();
            for (var i = 0; i < _addtionalDLL.childCount - 1; i++)
            {
                if(_addtionalDLL[i] is TextField dllPath && dllPath.value != "")
                    config.AOTDLLList.Add(dllPath.value);
            }

            if (!Directory.Exists(Application.dataPath + "/" + "Editor"))
                Directory.CreateDirectory(Application.dataPath + "/" + "Editor");

            _config = config;
            AssetDatabase.CreateAsset(config, _configPath);
            AssetDatabase.SaveAssets();
        }

        private void ClearOutPathClick()
        {
            ClearOutPath();
        }

        public void ClearOutPath(bool isTip = true)
        {
            var path = Application.dataPath + "/" + _outPath.value;
            Debug.Log(path);
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                for (var i = 0; i < files.Length; i++)
                {
                    if (Path.GetExtension(files[i]).ToLower() == ".dll" ||
                        Path.GetFileName(files[i]) == ".gitignore") //不删除dll文件和.gitignore文件
                    {
                        continue;
                    }

                    File.Delete(files[i]);
                    Debug.Log(files[i]);
                }
            }

            SaveConfig();
            AssetDatabase.Refresh();

            if (isTip)
            {
                EditorUtility.DisplayDialog("清理完成", "清理完成", "确定");
            }
        }

        public void ExportAll()
        {
            var path = Application.dataPath + "/" + _outPath.value;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

           
            ClearOutPath(false);
            
            File_BuildKey.Clear();
            foreach (var singleAbContent in _singleABContents)
            {
                if(singleAbContent.Value._exportToggle.value)
                    singleAbContent.Value.Export();
            }
            
            Builds.Clear();
            var packageData = new ABPackageData();
            
            var build_AddressableNamesDic = new Dictionary<string, List<string>>();
            var build_AseetNamesDic = new Dictionary<string, List<string>>();
            
            
            foreach (var keyValuePair in File_BuildKey)
            {
                var buildKey = MD5Helper.MD5(string.Join("_", keyValuePair.Value));
                
                //每一个路径需要一个PackageData来记录
                foreach (var packagePath in keyValuePair.Value)
                {
                    var package = AddPackageData(packagePath, packageData);
                    
                    if (keyValuePair.Value.Count > 1)
                    {
                        package.additionalFiles.Add(keyValuePair.Key.Replace("\\","/"));
                        package.additionalPackages.Add(buildKey);
                    }
                }
                
                AddPackageData(string.Join("_", keyValuePair.Value), packageData);

                if (!build_AddressableNamesDic.ContainsKey(buildKey))
                {
                    build_AddressableNamesDic.Add(buildKey,new List<string>());
                    build_AseetNamesDic.Add(buildKey,new List<string>());
                }
                build_AddressableNamesDic[buildKey].Add(Path.GetFileName(keyValuePair.Key));
                build_AseetNamesDic[buildKey].Add(keyValuePair.Key);
            }
            packageData.ManifestABName = Path.GetFileName(_outPath.value);

            foreach (var buildKey in build_AddressableNamesDic.Keys)
            {
                Builds.Add(new AssetBundleBuild()
                {
                    assetBundleName = buildKey,
                    assetNames = build_AseetNamesDic[buildKey].ToArray(),
                    addressableNames = build_AddressableNamesDic[buildKey].ToArray()
                });
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var manifest = BuildPipeline.BuildAssetBundles(path, Builds.ToArray(), BuildAssetBundleOptions.None,
                EditorUserBuildSettings.activeBuildTarget);

            
            //保存依赖
            foreach (var package in packageData.Packages)
            {
                package.dependencies.AddRange(manifest.GetAllDependencies(package.PackageID));
            }
            
            
            var packageDataPath = Path.Combine(path, "packagedata.json");
            
            if(File.Exists(packageDataPath))
                File.Delete(packageDataPath);
            
            File.Create(packageDataPath).Close();
            File.WriteAllText(packageDataPath, JsonUtility.ToJson(packageData), Encoding.UTF8);
            
            
            CopyDLL();
        }
        private void CaculateMD5()
        {
            var path = Application.dataPath + "/" + _outPath.value;

            //计算MD5
            var md5Path = Application.dataPath + "/" + _outPath.value +
                          "/" + md5FileName;
            FileMD5 fileMD5 = new FileMD5();
            var files = Directory.GetFiles(path);
            for (var i = 0; i < files.Length; i++)
            {
                if (Directory.Exists(files[i]) || Path.GetExtension(files[i]).ToLower() == ".meta" ||
                    Path.GetFileName(files[i]) == ".gitignore" || Path.GetFileName(files[i]) == versionFileName ||
                    Path.GetFileName(files[i]) == md5FileName)
                {
                    continue;
                }

                fileMD5.Files.Add(Path.GetFileName(files[i]));
                fileMD5.MD5.Add(MD5Helper.MD5ile(files[i]));
            }

            if (File.Exists(md5Path))
                File.Delete(md5Path);

            File.Create(md5Path).Close();
            File.WriteAllText(md5Path, JsonUtility.ToJson(fileMD5), Encoding.UTF8);
        }

        private ABPackage AddPackageData(string packageName, ABPackageData packageData)
        {
            ABPackage package = null;
            foreach (var t in packageData.Packages)
            {
                if (t.PackageName == packageName)
                {
                    package = t;
                    break;
                }
            }

            if (package == null)
            {
                package = new ABPackage()
                {
                    PackageName = packageName,
                    PackageID = MD5Helper.MD5(packageName),
                    additionalFiles = new List<string>(),
                    additionalPackages = new List<string>()
                };
                packageData.Packages.Add(package);
            }

            return package;
        }

        private void Compare()
        {
            if (!EditorUtility.DisplayDialog("分支确认",
                    $"现在要对比的分支为【{view.Q<DropdownField>("BranchDropDown").value}】，是否是想要对比的分支？", "是的", "终止"))
            {
                return;
            }


            bool hasComparePath = _comparePath.value != "";

            var comparePath = Application.dataPath + "/" + _comparePath.value;

            if (hasComparePath)
            {
                if (Directory.Exists(comparePath))
                    Directory.Delete(comparePath, true);
                Directory.CreateDirectory(comparePath);
            }

            if (File.Exists(serverVersionFilePath))
                File.Delete(serverVersionFilePath);
            //下载服务器version
            if (_serverVersionPath.value != "")
                DownloadServerVersion();

            if (File.Exists(serverMD5FilePath))
                File.Delete(serverMD5FilePath);
            //下载服务器version
            if (_serverMD5Path.value != "")
                DownloadServerMD5();

            ContinueCompare = DoContinueCompare;
            //检查服务器版本并询问新的版本号
            if (File.Exists(serverVersionFilePath))
            {
                var version = File.ReadAllText(serverVersionFilePath);
                ScriptableWizard.DisplayWizard<VersionConfirmDialog>(
                    $"检测到服务器版本为【{version}】，请输入新版本号", "确认").新版本号 = version;
            }
            else
            {
                ScriptableWizard.DisplayWizard<VersionConfirmDialog>(
                    $"没有检测到服务版本，请输入新版本号", "确认").新版本号 = "V1.0.0";
            }
        }

        private void FirstVersion()
        {
            if (!EditorUtility.DisplayDialog("首包确认", $"现在要发布的是首包？", "是的", "终止"))
            {
                return;
            }

            bool hasComparePath = _comparePath.value != "";

            var comparePath = Application.dataPath + "/" + _comparePath.value;

            if (hasComparePath)
            {
                if (Directory.Exists(comparePath))
                    Directory.Delete(comparePath, true);
                Directory.CreateDirectory(comparePath);
            }

            if (File.Exists(serverVersionFilePath))
                File.Delete(serverVersionFilePath);

            if (File.Exists(serverMD5FilePath))
                File.Delete(serverMD5FilePath);

            ContinueCompare = DoContinueCompare;

            ScriptableWizard.DisplayWizard<VersionConfirmDialog>(
                $"请输入新版本号", "确认").新版本号 = "V1.0.0";
        }

        private void DoContinueCompare(string newVersion)
        {
            bool hasComparePath = _comparePath.value != "";
            var path = Application.dataPath + "/" + _outPath.value;
            var comparePath = Application.dataPath + "/" + _comparePath.value;
            string[] files;

            //version
            var outPathVersion = Path.Combine(path,versionFileName);
            if (File.Exists(outPathVersion))
                File.Delete(outPathVersion);
            File.Create(outPathVersion).Close();
            File.WriteAllText(outPathVersion, newVersion);

            var newFileMD5Path = path + "/" + md5FileName;
            var newFileMD5 = JsonUtility.FromJson<FileMD5>(File.ReadAllText(newFileMD5Path, Encoding.UTF8));
            StringBuilder sb = new StringBuilder("差异文件：\r\n");
            if (!File.Exists(serverMD5FilePath))
            {
                if (hasComparePath)
                {
                    files = Directory.GetFiles(path);
                    for (var i = 0; i < files.Length; i++)
                    {
                        if (Directory.Exists(files[i]) || Path.GetExtension(files[i]).ToLower() == ".meta" ||
                            Path.GetFileName(files[i]) == ".gitignore")
                        {
                            continue;
                        }

                        var fileName = Path.GetFileName(files[i]);
                        var newPath = comparePath + "/" + fileName;
                        File.Copy(files[i], newPath);
                        sb.Append(Path.GetFileName(files[i]));
                        sb.Append("\r\n");
                    }
                }

                newFileMD5.AppVersion = newVersion;
            }
            else
            {
                var serverMD5 = JsonUtility.FromJson<FileMD5>(File.ReadAllText(serverMD5FilePath, Encoding.UTF8));

                //对比差异
                for (var i = 0; i < newFileMD5.Files.Count; i++)
                {
                    var fileName = newFileMD5.Files[i];
                    var newMd5 = newFileMD5.MD5[i];
                    if (!serverMD5.Files.Contains(fileName) ||
                        !newMd5.Equals(serverMD5.MD5[serverMD5.Files.IndexOf(fileName)]))
                    {
                        if (hasComparePath)
                            File.Copy(path + "/" + fileName,
                                comparePath + "/" + fileName);
                        sb.Append(fileName);
                        sb.Append("\r\n");
                    }
                }

                newFileMD5.AppVersion = newVersion;
            }

            File.WriteAllText(newFileMD5Path, JsonUtility.ToJson(newFileMD5), Encoding.UTF8);

            AssetDatabase.Refresh();


            EditorUtility.DisplayDialog("对比完成", sb.ToString(), "确定");
        }
    }

    public class SingleABContent
    {
        private Button _exportBtn;
        public TextField _name;
        public TextField _filePath;
        public TextField _matchRegex;
        public Toggle _exportToggle;
        private TextField _outPath;
        

        private ABToolEditor _editor;

        public SingleABContent(ABToolEditor editor)
        {
            _editor = editor;
            
            _name = new TextField();
            _name.style.borderTopColor = Color.gray;
            _name.style.borderTopWidth = 3;
            _name.label = "备注";
            _name.value = "未命名";
            editor._exportPaths.Insert(editor._exportPaths.childCount - 1, _name);
            
            _filePath = new TextField();
            _filePath.label = "导出路径";
            _filePath.value = "";
            editor._exportPaths.Insert(editor._exportPaths.childCount - 1 , _filePath);
            
            _matchRegex = new TextField();
            _matchRegex.label = "匹配正则";
            _matchRegex.value = "/*.(prefab|meta)$";
            editor._exportPaths.Insert(editor._exportPaths.childCount - 1, _matchRegex);
            
            
            _exportToggle = new Toggle();
            _exportToggle.label = "启用";
            _exportToggle.value = true;
            editor._exportPaths.Insert(editor._exportPaths.childCount - 1, _exportToggle);

            _outPath = editor._outPath;
        }
        public void Export()
        {
            var path = Application.dataPath + "/" + _filePath.value;
            Debug.Log(path);
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                var file = files[i];
                file = file.Replace(Path.DirectorySeparatorChar, '/');
                if (Regex.IsMatch(file.ToLower(), _matchRegex.value))
                    AddFile(file);
            }
        }

        private void AddFile(string file)
        {
             string buildKey = "";
            var addtionalPath = Application.dataPath + "/";
            var abstractPath = file.Replace(addtionalPath, "");
            if (Path.GetDirectoryName(file) == addtionalPath + _filePath.value)
            {
                buildKey = _filePath.value.Replace('/', '_');
            }
            else
            {
                var temp = Path.GetDirectoryName(abstractPath).Replace(Path.DirectorySeparatorChar, '_');
                buildKey = temp;
            }

            buildKey = buildKey.ToLower();
            
#if UNITY_IPHONE
        buildKey += "ios";
#elif UNITY_ANDROID
        buildKey += "android";
#elif UNITY_STANDALONE_WIN
            buildKey += "windows";
#elif UNITY_STANDALONE_OSX
        buildKey += "mac";
#endif

            buildKey = _editor._config.AppID + buildKey;

            AddFile(buildKey, "Assets" + "/" + abstractPath);
            

            if(Path.GetExtension(file).ToLower() == ".unity")
                return;//场景不需要依赖
            
            //遍历依赖文件
            var dependencies = AssetDatabase.GetDependencies(file.Replace(Application.dataPath, "Assets"));
            for (var i = 0; i < dependencies.Length; i++)
            {
                var dp = dependencies[i];
                if( dp.EndsWith(".cs") || dp.EndsWith(".shader"))
                    continue;
                
                AddFile(buildKey, dp);
            }
        }

        private void AddFile(string buildKey,string fileAbstractPath)
        {
            fileAbstractPath = fileAbstractPath.Replace(Path.DirectorySeparatorChar, '/');
            if(_editor.File_BuildKey.ContainsKey(fileAbstractPath))
            {
                if (!_editor.File_BuildKey[fileAbstractPath].Contains(buildKey))
                    _editor.File_BuildKey[fileAbstractPath].Add(buildKey);
            }
            else
                _editor.File_BuildKey.Add(fileAbstractPath,new List<string>(){buildKey});
            
        }
    }
}