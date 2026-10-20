using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ConfigEditor : Editor
{
    [MenuItem("RebornFrame/配置/生成配置文件", false, 40)]
    public static void BuildConfigManager()
    {
#if USE_AB
        string proto_cs_path = Application.dataPath + "/HotUpdate/Configs/AutoConfigClass/";
        string cfg_mgr_path = Application.dataPath + "/HotUpdate/Configs/AutoConfigManager/";
#else
        string proto_cs_path = Application.dataPath + "/Scripts/Configs/AutoConfigClass/";
        string cfg_mgr_path = Application.dataPath + "/Scripts/Configs/AutoConfigManager/";
#endif

        if (!Directory.Exists(proto_cs_path))
        {
            Directory.CreateDirectory(proto_cs_path);
        }

        if (!Directory.Exists(cfg_mgr_path))
        {
            Directory.CreateDirectory(cfg_mgr_path);
        }

        var proto_cs_list = Directory.GetFiles(proto_cs_path, "*.cs", SearchOption.AllDirectories);

        var cfg_mgr_list = Directory.GetFiles(cfg_mgr_path, "*Manager.cs", SearchOption.AllDirectories);
        List<string> config_mgr_list = new List<string>();
        foreach (var path in cfg_mgr_list)
            config_mgr_list.Add(path);

        foreach (var path in proto_cs_list)
        {
            var file_name = Path.GetFileNameWithoutExtension(path);
            var file = config_mgr_list.Find(item =>
            {
                return Path.GetFileNameWithoutExtension(item).Replace("Manager", string.Empty).ToLower() ==
                       file_name.ToLower();
            });
            if (file == null)
            {
                BuildSingleConfigManager(path, cfg_mgr_path);
            }
        }

        AssetDatabase.Refresh();
    }

    private static void BuildSingleConfigManager(string path, string mgr_path)
    {
        StreamReader sr = new StreamReader(path, Encoding.Default);
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            //public sealed class Drop : pb::IMessage

            if (line.Contains("  public sealed partial class "))
            {
                var temp = line.Split(':');
                string name = temp[0].Replace("  public sealed partial class ", string.Empty)
                    .Replace(" : pb::IMessage", string.Empty).Replace(" ", string.Empty);
                FileStream creat = new FileStream(string.Format("{0}{1}Manager.cs", mgr_path, name),
                    FileMode.Create);
                creat.Close();

                TextAsset textAsset = (TextAsset)AssetDatabase.LoadAssetAtPath("Packages/com.reborn.reborncore/Editor/Config/ExampleConfigManager.cs.txt", typeof(TextAsset));
                var content = textAsset.text.Replace("Example", name);
                StreamWriter sw = File.AppendText(string.Format("{0}{1}Manager.cs", mgr_path, name));
                sw.Write(content);
                sw.Flush();
                sw.Close();
                sw.Dispose();
                
                Debug.Log($"Creat {name} Success <color=#FF0000>Need Register DataConfigManager</color>");
                break;
            }
        }
    }
}